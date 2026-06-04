import { useMutation, useQuery } from "@tanstack/react-query";
import {
  AlertCircle,
  ArrowRight,
  Check,
  ChevronDown,
  Eye,
  EyeOff,
  Loader2,
  UserPlus,
} from "lucide-react";
import { useEffect, useMemo, useState, type FormEvent } from "react";
import { Link, Navigate } from "react-router-dom";
import { registerUser } from "@/api/identity";
import { listTenantsPublic } from "@/api/tenants";
import { useAuth } from "@/auth/use-auth";
import { AuthHeadline, AuthShell } from "@/components/auth/auth-shell";
import { Button } from "@/components/ui/button";
import { DropdownMenu, DropdownMenuTrigger, DropdownMenuItem, DropdownMenuContent } from "@/components/ui/dropdown-menu";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { setPendingReferralHighlight } from "@/hooks/use-referral";
import { ApiRequestError } from "@/lib/api-client";
import { cn } from "@/lib/cn";

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
  const [tenant, setTenant] = useState("");
  const [tenantSearch, setTenantSearch] = useState("");
  const [password, setPassword] = useState("");
  const [confirmPassword, setConfirmPassword] = useState("");
  const [error, setError] = useState<string | null>(null);
  const [showPassword, setShowPassword] = useState(false);
  const [showConfirm, setShowConfirm] = useState(false);
  const [submitted, setSubmitted] = useState(false);

  const { data: tenants = [], isLoading: loadingTenants } = useQuery({
    queryKey: ["tenants"],
    queryFn: listTenantsPublic,
    staleTime: 5 * 60 * 1000, // 5 minutes
  });
  const strength = useMemo(() => scorePassword(password), [password]);
  const passwordsMatch = password.length > 0 && password === confirmPassword;
  const selectedTenant = tenants.find((t) => t.id === tenant);
  // Get referral code from localStorage (persistent) or URL params
  const [referralCode, setReferralCode] = useState<string>("");
  const [storedReferralCodes, setStoredReferralCodes] = useState<string[]>([]);

  useEffect(() => {
    const stored = localStorage.getItem("fsh.dashboard.referralCodes");
    const codes = stored ? JSON.parse(stored) as string[] : [];
    setStoredReferralCodes(Array.isArray(codes) ? codes : []);
    // Use the first referral code if available
    if (codes.length > 0 && !referralCode) {
      setReferralCode(codes[0]);
    }
  }, []);

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
        referralCode: referralCode || undefined,
      }),
    onSuccess: () => {
      setSubmitted(true);
      // Set pending highlight for post-registration navigation
      if (storedReferralCodes.length > 0) {
        setPendingReferralHighlight(true);
      }
    },
    onError: (err: unknown) => {
      const detail =
        err instanceof ApiRequestError
          ? err.problem?.detail ?? err.problem?.title ?? err.message
          : (err as Error).message;
      setError(detail);
    },
  });

  // Clear error when form fields change
  useEffect(() => {
    setError(null);
  }, [firstName, lastName, email, userName, tenant, password, confirmPassword]);

  if (isAuthenticated) {
    return <Navigate to="/" replace />;
  }

  const onSubmit = async (e: FormEvent<HTMLFormElement>) => {
    e.preventDefault();
    if (!passwordsMatch) {
      setError("Passwords don't match.");
      return;
    }
    if (password.length < 8) {
      setError("Use at least 8 characters.");
      return;
    }
    if (!tenant) {
      setError("Please select a tenant.");
      return;
    }
    mutation.mutate();
  };

  // Filter tenants based on search input
  const filteredTenants = tenantSearch
    ? tenants.filter((t) => t.id.toLowerCase().includes(tenantSearch.toLowerCase()) || t.name?.toLowerCase().includes(tenantSearch.toLowerCase()))
    : tenants;

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
              If the tenant <span className="text-[var(--color-foreground)]">{selectedTenant?.name ?? tenant}</span> exists and registrations are enabled, you'll receive a confirmation link shortly.
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

          <form onSubmit={onSubmit} className="space-y-5" noValidate aria-describedby={error ? "register-error" : undefined}>
            {/* Tenant Combobox */}
            <div className="space-y-1.5">
              <Label
                htmlFor="tenant"
                className="block text-[11.5px] font-semibold uppercase tracking-wider text-[var(--color-muted-foreground)]"
              >
                Tenant
              </Label>
              <DropdownMenu>
                <DropdownMenuTrigger asChild>
                  <div className="relative">
                    <Input
                      id="tenant"
                      value={tenantSearch}
                      onChange={(e) => setTenantSearch(e.target.value)}
                      placeholder={loadingTenants ? "Loading tenants..." : "Select or type tenant..."}
                      autoComplete="organization"
                      required
                      aria-invalid={error ? true : undefined}
                      aria-describedby={error ? "register-error" : undefined}
                      className="h-11 pr-11 text-[14px]"
                      disabled={loadingTenants}
                    />
                    <ChevronDown className="pointer-events-none absolute right-3.5 top-1/2 size-4 -translate-y-1/2 text-[var(--color-muted-foreground)]" />
                  </div>
                </DropdownMenuTrigger>
                <DropdownMenuContent className="max-h-60 w-[var(--radix-dropdown-menu-trigger-width)]" align="start">
                  {filteredTenants.length === 0 ? (
                    <div className="px-3 py-2 text-[11.5px] text-[var(--color-muted-foreground)]">
                      No tenants found
                    </div>
                  ) : (
                    filteredTenants.map((t) => (
                      <DropdownMenuItem
                        key={t.id}
                        onSelect={() => {
                          setTenant(t.id);
                          setTenantSearch(t.id);
                        }}
                      >
                        {t.id} {t.name && <span className="text-[var(--color-muted-foreground)]">- {t.name}</span>}
                      </DropdownMenuItem>
                    ))
                  )}
                </DropdownMenuContent>
              </DropdownMenu>
            </div>

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
                aria-invalid={error ? true : undefined}
                className="h-11 text-[14px]"
              />
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
                aria-invalid={error ? true : undefined}
                className="h-11 text-[14px]"
              />
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
                required
                aria-invalid={error ? true : undefined}
                className="h-11 text-[14px]"
              />
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
                aria-invalid={error ? true : undefined}
                className="h-11 text-[14px]"
              />
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
                  autoFocus
                  minLength={8}
                  aria-invalid={error ? true : undefined}
                  aria-describedby={error ? "register-error" : undefined}
                  className="h-11 pr-11 text-[14px]"
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
                  aria-invalid={error ? true : undefined}
                  aria-describedby={error ? "register-error" : undefined}
                  className="h-11 pr-11 text-[14px]"
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

              {confirmPassword.length > 0 && (
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

            {error && (
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
                <span className="leading-snug">{error}</span>
              </div>
            )}

            <div className="pt-1.5">
              <Button
                type="submit"
                disabled={mutation.isPending || !firstName || !lastName || !email || !userName || !tenant || !passwordsMatch || password.length < 8}
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