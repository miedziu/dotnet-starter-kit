import { useEffect } from "react";
import { useNavigate, useSearchParams } from "react-router-dom";
import { addReferralCode } from "@/hooks/use-referral";

export function ReferralRedirectPage() {
  const [searchParams] = useSearchParams();
  const navigate = useNavigate();

  useEffect(() => {
    const code = searchParams.get("c");
    if (code) {
      addReferralCode(code);
    }
    // Redirect to register page
    navigate("/register", { replace: true });
  }, [searchParams, navigate]);

  return null;
}