const REFERRAL_KEY = "fsh.dashboard.referralCodes";
const PENDING_HIGHLIGHT_KEY = "fsh.dashboard.pendingReferralHighlight";

// Get all stored referral codes for the current user
export function getReferralCodes(): string[] {
  const stored = localStorage.getItem(REFERRAL_KEY);
  if (!stored) return [];
  try {
    const codes = JSON.parse(stored) as string[];
    return Array.isArray(codes) ? codes : [];
  } catch {
    return [];
  }
}

// Add a referral code (append if not already present)
export function addReferralCode(code: string): void {
  if (!code || code.length === 0) return;
  
  const existing = getReferralCodes();
  if (!existing.includes(code)) {
    const updated = [...existing, code];
    localStorage.setItem(REFERRAL_KEY, JSON.stringify(updated));
  }
}

// Clear all referral codes
export function clearReferralCodes(): void {
  localStorage.removeItem(REFERRAL_KEY);
}

// Set pending highlight flag for post-registration navigation
export function setPendingReferralHighlight(hasCodes: boolean): void {
  if (hasCodes) {
    localStorage.setItem(PENDING_HIGHLIGHT_KEY, "true");
  } else {
    localStorage.removeItem(PENDING_HIGHLIGHT_KEY);
  }
}

// Check and clear pending highlight flag
export function checkAndClearPendingReferralHighlight(): boolean {
  const hasHighlight = localStorage.getItem(PENDING_HIGHLIGHT_KEY) === "true";
  if (hasHighlight) {
    localStorage.removeItem(PENDING_HIGHLIGHT_KEY);
  }
  return hasHighlight;
}

// React hook for referral codes
import { useState, useEffect } from "react";

export function useReferralCodes() {
  const [codes, setCodes] = useState<string[]>([]);
  
  useEffect(() => {
    setCodes(getReferralCodes());
  }, []);
  
  const addCode = (code: string) => {
    addReferralCode(code);
    setCodes(getReferralCodes());
  };
  
  return { codes, addCode, clear: clearReferralCodes };
}