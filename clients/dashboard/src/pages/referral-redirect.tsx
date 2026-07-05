import { useEffect } from "react";
import { useParams, useNavigate } from "react-router-dom";
import { addReferralUsername } from "@/hooks/use-referral";

export function ReferralRedirectPage() {
  const { username } = useParams<{ username: string }>();
  const navigate = useNavigate();

  useEffect(() => {
    if (username) {
      addReferralUsername(username);
    }
    // Redirect to register page
    navigate("/register", { replace: true });
  }, [username, navigate]);

  return null;
}