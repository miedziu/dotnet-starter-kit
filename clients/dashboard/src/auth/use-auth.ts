import { useContext } from "react";
import { AuthContext } from "@/auth/auth-context";

export function useAuth() {
  const ctx = useContext(AuthContext);
  if (!ctx) {
    throw new Error("useAuth must be used within AuthProvider");
  }
  return ctx;
}

export function useReferralHighlight() {
  const ctx = useContext(AuthContext);
  if (!ctx) {
    throw new Error("useReferralHighlight must be used within AuthProvider");
  }
  return ctx.user?.hasReferralHighlight ?? false;
}
