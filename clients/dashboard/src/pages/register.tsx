import { useMutation } from "@tanstack/react-query";
import {
  AlertCircle,
  ArrowRight,
  Check,
  Eye,
  EyeOff,
  Loader2,
  UserPlus,
} from "lucide-react";
import { useEffect, useMemo, useState, type FormEvent } from "react";
import { Link, Navigate } from "react-router-dom";
import { registerUser } from "@/api/identity";
import { useAuth } from "@/auth/use-auth";
import { AuthHeadline, AuthShell } from "@/components/auth/auth-shell";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { setPendingReferralHighlight } from "@/hooks/use-referral";
import { ApiRequestError } from "@/lib/api-client";
import { cn } from "@/lib/cn";

// Field-level errors from FluentValidation are keyed by property name
type FieldErrors = Record<string, string[] | undefined>;

// ────────────────────────────────────────────────────────────────────────────
// Password strength scoring (matches reset-password.tsx style)
// ────────────────────────────────────────────────────────────────────────────

type Strength = "weak" | "fair" | "strong";

function scorePassword(value: string): Strength | null {
  if (value.length === 0) return null;
  if (value.length < 8) return "weak";

  let score = 0;
  if (/[a-z]/.test(value)) score++;
  if (/[A-Z]/.test(value)) score++;
  if (/\d/.test(value)) score++;
  if (/[^A-Za-z0-9]/.test(value)) score++;
  if (value.length >= 12) score++;

  if (score <= 2) return "weak";
  if (score === 3) return "fair";
  return "strong";
}

const STRENGTH_META: Record<Strength, { label: string; fill: string; bar: string }> = {
  weak: {
    label: "Weak",
    fill: "bg-[var(--color-destructive)]",
    bar: "w-1/3",
  },
  fair: {
    label: "Fair",
    fill: "bg-[var(--color-warning)]",
    bar: "w-2/3",
  },
  strong: {
    label: "Strong",
    fill: "bg-[var(--color-success)]",
    bar: "w-full",
  },
};

// ────────────────────────────────────────────────────────────────────────────
// RegisterPage — creates a new user account
// ────────────────────────────────────────────────────────────────────────────

export function RegisterPage() {
  const { isAuthenticated } = useAuth();

  const [firstName, setFirstName] = useState("");
  const [lastName, setLastName] = useState("");
  const [email, setEmail] = useState("");
  const [userName, setUserName] = useState("");
  const [password, setPassword] = useState("");
  const [confirmPassword, setConfirmPassword] = useState("");
  const [error, setError] = useState<string[]>([]);
  // Field-level errors keyed by field name (from FluentValidation)
  const [fieldErrors, setFieldErrors] = useState<FieldErrors>({});
  const [showPassword, setShowPassword] = useState(false);
  const [showConfirm, setShowConfirm] = useState(false);
  const [submitted, setSubmitted] = useState(false);

  const strength = useMemo(() => scorePassword(password), [password]);
  const passwordsMatch = password.length > 0 && password === confirmPassword;

  // Get referral usernames from localStorage
  const [storedReferralUsernames, setStoredReferralUsernames] = useState<string[]>([]);

  useEffect(() => {
    const stored = localStorage.getItem("fsh.dashboard.referralUsernames");
    const usernames = stored ? JSON.parse(stored) as string[] : [];
    setStoredReferralUsernames(Array.isArray(usernames) ? usernames : []);
  }, []);

  // Helper function to check if a field has validation errors
  function getFieldError(field: string): string | undefined {
    const errors = fieldErrors[field];
    return errors?.[0];
  }

  // Helper function to extract field-level errors from API response
  function extractFieldErrors(err: unknown): FieldErrors {
    if (err instanceof ApiRequestError && err.problem?.errors) {
      const errors = err.problem.errors;
      if (Array.isArray(errors)) {
        // Flat array of errors - no field mapping available
        return {};
      } else {
        // Record keyed by field name (FluentValidation format)
        return errors;
      }
    }
    return {};
  }

  // Helper function to extract all errors from API response
  function getErrorsFromResponse(err: unknown): string[] {
    if (err instanceof ApiRequestError && err.problem) {
      // FluentValidation errors arrive as Record<string, string[]>
      const errors = err.problem.errors;
      if (errors) {
        if (Array.isArray(errors)) {
          // Flat array of errors
          return errors;
        } else {
          // Record keyed by field - extract all messages
          return Object.values(errors).flat();
        }
      }
      // Fall back to single error message
      return [err.problem.detail ?? err.problem.title ?? err.message];
    }
    if (err instanceof Error) {
      return [err.message];
    }
    return ["An unexpected error occurred"];
  }

  const mutation = useMutation({
    mutationFn: () =>
      registerUser({
        firstName,
        lastName,
        email,
        userName,
        password,
        confirmPassword,
        phoneNumber: "",
        referralUsernames: storedReferralUsernames.length > 0 ? storedReferralUsernames : undefined,
      }),
    onSuccess: () => {
      setSubmitted(true);
      // Set pending highlight for post-registration navigation
      if (storedReferralUsernames.length > 0) {
        setPendingReferralHighlight(true);
      }
    },
    onError: (err: unknown) => {
      setError(getErrorsFromResponse(err));
      setFieldErrors(extractFieldErrors(err));
    },
  });

  // Clear errors when form fields change
  useEffect(() => {
    setError([]);
    setFieldErrors({});
  }, [firstName, lastName, email, userName, password, confirmPassword]);

  if (isAuthenticated) {
    return <Navigate to="/" replace />;
  }

  const onSubmit = async (e: FormEvent<HTMLFormElement>) => {
    e.preventDefault();
    if (!passwordsMatch) {
      setError(["Passwords don't match."]);
      setFieldErrors({});
      return;
    }
    if (password.length < 8) {
      setError(["Use at least 8 characters."]);
      setFieldErrors({});
      return;
    }
    setError([]);
    setFieldErrors({});
    mutation.mutate();
  };

  return (
    <AuthShell
      footer={
        <span>
          Already have an account?{" "}
          <Link
            to="/login"
            className="text-[var(--color-foreground)] underline-offset-4 hover:underline"
          >
            Sign in
          </Link>
        </span>
      }
    >
      {submitted ? (
        <div className="fsh-enter space-y-5 text-center">
          <div className="grid place-items-center">
            <span
              aria-hidden
              className="grid size-14 place-items-center rounded-2xl bg-[oklch(from_var(--color-success)_l_c_h_/_0.10)] text-[var(--color-success)]"
            >
              <Check className="size-6" />
            </span>
          </div>
          <div>
            <AuthHeadline lead="Check your" accent="email" />
            <p className="text-[13px] leading-relaxed text-[var(--color-muted-foreground)]">
              If an account with this email exists and registrations are enabled, you'll receive a confirmation link shortly.
            </p>
          </div>
          <ul className="space-y-1.5 text-left text-[12.5px] text-[var(--color-muted-foreground)]">
            <li className="flex items-start gap-2">
              <Check className="mt-0.5 size-3.5 shrink-0 text-[var(--color-success)]" />
              Click the link to activate your account and set your password.
            </li>
            <li className="flex items-start gap-2">
              <Check className="mt-0.5 size-3.5 shrink-0 text-[var(--color-success)]" />
              Didn't get it? Wait a minute, then check spam.
            </li>
          </ul>
          <div className="flex items-center gap-2 pt-1">
            <Link to="/login" className="ml-auto">
              <Button type="button" variant="outline">
                Back to sign in
              </Button>
            </Link>
          </div>
        </div>
      ) : (
        <>
          <div className="mb-6 sm:mb-8">
            <AuthHeadline lead="Create an" accent="account" />
            <p className="text-[13px] text-[var(--color-muted-foreground)]">
              Fill in your details to get started
            </p>
          </div>

          <form onSubmit={onSubmit} className="space-y-5" noValidate aria-describedby={error.length > 0 ? "register-error" : undefined}>
            {/* First Name */}
            <div className="space-y-1.5">
              <Label
                htmlFor="firstName"
                className="block text-[11.5px] font-semibold uppercase tracking-wider text-[var(--color-muted-foreground)]"
              >
                First name
              </Label>
              <Input
                id="firstName"
                value={firstName}
                onChange={(e) => setFirstName(e.target.value)}
                placeholder="John"
                autoComplete="given-name"
                required
                aria-invalid={getFieldError("firstName") ? true : undefined}
                aria-describedby={getFieldError("firstName") ? "firstName-error" : undefined}
                className={cn("h-11 text-[14px]", getFieldError("firstName") && "border-destructive")}
              />
              {getFieldError("firstName") && (
                <p id="firstName-error" className="text-[12px] text-[var(--color-destructive)]">
                  {getFieldError("firstName")}
                </p>
              )}
            </div>

            {/* Last Name */}
            <div className="space-y-1.5">
              <Label
                htmlFor="lastName"
                className="block text-[11.5px] font-semibold uppercase tracking-wider text-[var(--color-muted-foreground)]"
              >
                Last name
              </Label>
              <Input
                id="lastName"
                value={lastName}
                onChange={(e) => setLastName(e.target.value)}
                placeholder="Doe"
                autoComplete="family-name"
                required
                aria-invalid={getFieldError("lastName") ? true : undefined}
                aria-describedby={getFieldError("lastName") ? "lastName-error" : undefined}
                className={cn("h-11 text-[14px]", getFieldError("lastName") && "border-destructive")}
              />
              {getFieldError("lastName") && (
                <p id="lastName-error" className="text-[12px] text-[var(--color-destructive)]">
                  {getFieldError("lastName")}
                </p>
              )}
            </div>

            {/* Email */}
            <div className="space-y-1.5">
              <Label
                htmlFor="email"
                className="block text-[11.5px] font-semibold uppercase tracking-wider text-[var(--color-muted-foreground)]"
              >
                Email
              </Label>
              <Input
                id="email"
                type="email"
                value={email}
                onChange={(e) => setEmail(e.target.value)}
                placeholder="john@example.com"
                autoComplete="email"
                autoFocus
                required
                aria-invalid={getFieldError("email") ? true : undefined}
                aria-describedby={getFieldError("email") ? "email-error" : undefined}
                className={cn("h-11 text-[14px]", getFieldError("email") && "border-destructive")}
              />
              {getFieldError("email") && (
                <p id="email-error" className="text-[12px] text-[var(--color-destructive)]">
                  {getFieldError("email")}
                </p>
              )}
            </div>

            {/* Username */}
            <div className="space-y-1.5">
              <Label
                htmlFor="userName"
                className="block text-[11.5px] font-semibold uppercase tracking-wider text-[var(--color-muted-foreground)]"
              >
                Username
              </Label>
              <Input
                id="userName"
                value={userName}
                onChange={(e) => setUserName(e.target.value)}
                placeholder="johndoe"
                autoComplete="username"
                required
                aria-invalid={getFieldError("userName") ? true : undefined}
                aria-describedby={getFieldError("userName") ? "userName-error" : undefined}
                className={cn("h-11 text-[14px]", getFieldError("userName") && "border-destructive")}
              />
              {getFieldError("userName") && (
                <p id="userName-error" className="text-[12px] text-[var(--color-destructive)]">
                  {getFieldError("userName")}
                </p>
              )}
            </div>

            {/* Password */}
            <div className="space-y-1.5">
              <Label
                htmlFor="password"
                className="block text-[11.5px] font-semibold uppercase tracking-wider text-[var(--color-muted-foreground)]"
              >
                Password
              </Label>
              <div className="relative">
                <Input
                  id="password"
                  type={showPassword ? "text" : "password"}
                  value={password}
                  onChange={(e) => setPassword(e.target.value)}
                  placeholder="At least 8 characters"
                  autoComplete="new-password"
                  required
                  minLength={8}
                  aria-invalid={getFieldError("password") ? true : error.length > 0 ? true : undefined}
                  aria-describedby={getFieldError("password") ? "password-error" : error.length > 0 ? "register-error" : undefined}
                  className={cn("h-11 pr-11 text-[14px]", getFieldError("password") && "border-destructive")}
                />
                <button
                  type="button"
                  onClick={() => setShowPassword((v) => !v)}
                  aria-label={showPassword ? "Hide password" : "Show password"}
                  className="absolute right-3.5 top-1/2 grid h-6 w-6 -translate-y-1/2 cursor-pointer place-items-center rounded text-[var(--color-muted-foreground)] transition-colors hover:text-[var(--color-foreground)] focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-[var(--color-ring)]"
                >
                  {showPassword ? <EyeOff className="size-4" /> : <Eye className="size-4" />}
                </button>
              </div>

              {getFieldError("password") && (
                <p id="password-error" className="text-[12px] text-[var(--color-destructive)]">
                  {getFieldError("password")}
                </p>
              )}

              {strength && (
                <div className="fsh-enter flex items-center gap-2 pt-1.5">
                  <div className="h-1 flex-1 overflow-hidden rounded-full bg-[var(--color-muted)]">
                    <div
                      className={cn(
                        "h-full transition-all duration-200",
                        STRENGTH_META[strength].fill,
                        STRENGTH_META[strength].bar,
                      )}
                    />
                  </div>
                  <span className="min-w-[3.5rem] text-right text-[10px] font-semibold uppercase tracking-wider text-[var(--color-muted-foreground)]">
                    {STRENGTH_META[strength].label}
                  </span>
                </div>
              )}
            </div>

            {/* Confirm Password */}
            <div className="space-y-1.5">
              <Label
                htmlFor="confirmPassword"
                className="block text-[11.5px] font-semibold uppercase tracking-wider text-[var(--color-muted-foreground)]"
              >
                Confirm password
              </Label>
              <div className="relative">
                <Input
                  id="confirmPassword"
                  type={showConfirm ? "text" : "password"}
                  value={confirmPassword}
                  onChange={(e) => setConfirmPassword(e.target.value)}
                  placeholder="Re-enter password"
                  autoComplete="new-password"
                  required
                  minLength={8}
                  aria-invalid={getFieldError("confirmPassword") ? true : error.length > 0 ? true : undefined}
                  aria-describedby={getFieldError("confirmPassword") ? "confirmPassword-error" : error.length > 0 ? "register-error" : undefined}
                  className={cn("h-11 pr-11 text-[14px]", getFieldError("confirmPassword") && "border-destructive")}
                />
                <button
                  type="button"
                  onClick={() => setShowConfirm((v) => !v)}
                  aria-label={showConfirm ? "Hide password" : "Show password"}
                  className="absolute right-3.5 top-1/2 grid h-6 w-6 -translate-y-1/2 cursor-pointer place-items-center rounded text-[var(--color-muted-foreground)] transition-colors hover:text-[var(--color-foreground)] focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-[var(--color-ring)]"
                >
                  {showConfirm ? <EyeOff className="size-4" /> : <Eye className="size-4" />}
                </button>
              </div>

              {getFieldError("confirmPassword") && (
                <p id="confirmPassword-error" className="text-[12px] text-[var(--color-destructive)]">
                  {getFieldError("confirmPassword")}
                </p>
              )}

              {confirmPassword.length > 0 && !getFieldError("confirmPassword") && (
                <div
                  className={cn(
                    "flex items-center gap-1.5 pt-1 text-[11.5px]",
                    passwordsMatch
                      ? "text-[var(--color-success)]"
                      : "text-[var(--color-muted-foreground)]",
                  )}
                >
                  <Check
                    className={cn(
                      "size-3.5",
                      passwordsMatch ? "opacity-100" : "opacity-40",
                    )}
                  />
                  <span>{passwordsMatch ? "Passwords match" : "Doesn't match yet"}</span>
                </div>
              )}
            </div>

            {error.length > 0 && (
              <div
                id="register-error"
                role="alert"
                className={cn(
                  "fsh-enter flex items-start gap-2 rounded-lg border px-3 py-2 text-sm",
                  "border-[oklch(from_var(--color-destructive)_l_c_h_/_0.30)]",
                  "bg-[oklch(from_var(--color-destructive)_l_c_h_/_0.06)]",
                  "text-[var(--color-destructive)]",
                )}
              >
                <AlertCircle className="mt-0.5 size-4 shrink-0" />
                <ul className="list-disc list-inside space-y-1">
                  {error.map((msg, idx) => (
                    <li key={idx} className="leading-snug">{msg}</li>
                  ))}
                </ul>
              </div>
            )}

            <div className="pt-1.5">
              <Button
                type="submit"
                disabled={mutation.isPending || !firstName || !lastName || !email || !userName || !passwordsMatch || password.length < 8}
                className="group h-11 w-full text-[14px] font-semibold"
              >
                {mutation.isPending ? (
                  <>
                    <Loader2 className="size-4 animate-spin" />
                    <span>Creating account…</span>
                  </>
                ) : (
                  <>
                    <UserPlus className="size-4" />
                    <span>Create account</span>
                    <ArrowRight className="size-[14px] opacity-60 transition-all duration-200 group-hover:translate-x-0.5 group-hover:opacity-100" />
                  </>
                )}
              </Button>
            </div>
          </form>
        </>
      )}
    </AuthShell>
  );
}