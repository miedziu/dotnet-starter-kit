const REFERRAL_KEY = "fsh.dashboard.referralUsernames";
const PENDING_HIGHLIGHT_KEY = "fsh.dashboard.pendingReferralHighlight";
const MAX_REFERRAL_COUNT = 10;

// Get all stored referral usernames for the current user
export function getReferralUsernames(): string[] {
  const stored = localStorage.getItem(REFERRAL_KEY);
  if (!stored) return [];
  try {
    const usernames = JSON.parse(stored) as string[];
    return Array.isArray(usernames) ? usernames : [];
  } catch {
    return [];
  }
}

// Add a referral username (append if not already present)
// Limits the array to MAX_REFERRAL_COUNT, removing oldest entries first
export function addReferralUsername(username: string): void {
  if (!username || username.length === 0) return;

  const existing = getReferralUsernames();
  if (!existing.includes(username)) {
    const updated = [...existing, username];
    // Limit to MAX_REFERRAL_COUNT by removing oldest entries (from the beginning)
    const trimmed = updated.length > MAX_REFERRAL_COUNT
      ? updated.slice(updated.length - MAX_REFERRAL_COUNT)
      : updated;
    localStorage.setItem(REFERRAL_KEY, JSON.stringify(trimmed));
  }
}

// Set pending highlight flag for post-registration navigation
export function setPendingReferralHighlight(hasUsernames: boolean): void {
  if (hasUsernames) {
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