import { useMutation } from "@tanstack/react-query";
import { LogOut, ShieldAlert, UserCog } from "lucide-react";
import { useState } from "react";
import { useNavigate } from "react-router-dom";
import { toast } from "sonner";
import { useAuth } from "@/auth/use-auth";
import { Button } from "@/components/ui/button";
import { cn } from "@/lib/cn";

/**
 * Sticky banner shown across every page while the operator is impersonating
 * another user. Reads the act_* claims from the live access token via
 * AuthContext.impersonation.
 *
 * Design language matches the dashboard's editorial-console aesthetic:
 * soft tone wash (radial gradient, no alarm stripes), gradient-ring icon
 * square, mono small-caps meta label, display-font subject with gradient
 * accent, code-chip tenants, hairline tone border.
 *
 * Tone scaling:
 *   - same-tenant impersonation → warning (amber). Operator is acting
 *     inside their own tenant boundary; lower blast radius.
 *   - cross-tenant impersonation → destructive (red). A root/SuperAdmin
 *     reached into a different tenant; tone intensifies (stronger wash,
 *     left ribbon, slightly bolder copy).
 */
export function ImpersonationBanner() {
  const { impersonation, user, stopImpersonation } = useAuth();
  const navigate = useNavigate();
  const [pending, setPending] = useState(false);

  const stop = useMutation({
    mutationFn: async () => {
      setPending(true);
      try {
        return await stopImpersonation();
      } finally {
        setPending(false);
      }
    },
    onSuccess: (result) => {
      // No dashboard session to return to (root operator handed off from the
      // admin app) → land on /login. Otherwise the operator's own session was
      // restored → back to the dashboard home.
      if (result.signedOut) {
        toast.success("Impersonation ended");
        navigate("/login", { replace: true });
      } else {
        toast.success("Returned to your session");
        navigate("/", { replace: true });
      }
    },
    onError: (err) => {
      toast.error("Could not end impersonation cleanly", {
        description: err instanceof Error ? err.message : "Restored local session.",
      });
      navigate("/", { replace: true });
    },
  });

  if (!impersonation) return null;

  const subjectLabel = user?.name ?? user?.email ?? "Unknown";
  const actorLabel = impersonation.actorName ?? impersonation.actorUserId.slice(0, 8) + "…";

  // One CSS variable drives every tone-derived color in the bar so we can
  // flip between warning / destructive without scattering conditionals.
  const tone = "var(--color-warning)";
  const metaLabel = "Impersonating";

  return (
    <div
      role="status"
      aria-live="polite"
      className={cn(
        "relative z-40 flex flex-wrap items-center justify-between gap-3 overflow-hidden",
        "border-b px-4 py-3 sm:px-6",
      )}
      style={{
        borderColor: `oklch(from ${tone} l c h / 0.28)`,
        backgroundColor: "var(--color-muted)",
      }}
    >
      <div className="flex min-w-0 flex-wrap items-center gap-3">
        {/* Gradient-ring icon square — matches the SetupTile / topbar icon
            treatment elsewhere in the dashboard. */}
        <span
          aria-hidden
          className="grid h-8 w-8 shrink-0 place-items-center rounded-xl"
          style={{
            backgroundColor: `oklch(from ${tone} l c h / 0.14)`,
            color: tone,
            boxShadow: `inset 0 0 0 1px oklch(from ${tone} l c h / 0.32)`,
          }}
        >
          <ShieldAlert className="h-3.5 w-3.5" />
        </span>

        <div className="flex min-w-0 flex-wrap items-baseline gap-x-3 gap-y-1">
          <span
            className="text-[11px] font-semibold uppercase tracking-wider"
            style={{ color: tone }}
          >
            {metaLabel}
          </span>

          {/* Subject — the user being acted as. */}
          <span
            className={cn(
              "font-display truncate text-[14px] font-semibold leading-tight tracking-tight",
              "text-[var(--color-foreground)]",
            )}
          >
            {subjectLabel}
          </span>

          {/* Operator attribution — hidden on small screens to keep the
              bar to one line on mobile. The `acting as` phrasing covers
              both variants. */}
          <span className="hidden items-center gap-1 text-[12px] text-[var(--color-muted-foreground)] sm:inline-flex">
            <span>· operator</span>
            <UserCog className="h-3 w-3" aria-hidden />
            <span className="text-[var(--color-foreground)]">
              {actorLabel}
            </span>
          </span>
        </div>
      </div>

      <Button
        size="sm"
        variant="outline"
        onClick={() => stop.mutate()}
        disabled={pending}
        className="shrink-0"
        style={{
          borderColor: `oklch(from ${tone} l c h / 0.45)`,
          color: tone,
        }}
      >
        <LogOut className="mr-1.5 h-3.5 w-3.5" />
        {pending ? "Ending…" : "End impersonation"}
      </Button>
    </div>
  );
}