import { ArrowRight, BookOpen, CheckCircle2, Globe, Shield, Users } from "lucide-react";
import { Link } from "react-router-dom";
import { useAuth } from "@/auth/use-auth";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";

export function PublicPage() {
  const { user, isAuthenticated } = useAuth();

  // Show profile data when logged in
  if (isAuthenticated && user) {
    return (
      <div className="min-h-screen bg-gradient-to-b from-background via-background to-accent">
        {/* Header */}
        <header className="border-b border-border bg-card/50 backdrop-blur supports-[backdrop-filter]:bg-card/50">
          <div className="mx-auto max-w-7xl px-4 py-6 sm:px-6 lg:px-8">
            <div className="flex items-center justify-between">
              <div className="flex items-center gap-2">
                <Shield className="h-6 w-6 text-primary" />
                <span className="font-display text-xl font-bold">FSH Platform</span>
              </div>
              <div className="flex items-center gap-3">
                <span className="text-sm text-muted-foreground">
                  Signed in as {user.email}
                </span>
                <Link to="/dashboard">
                  <Button variant="outline" size="sm">
                    Go to Dashboard
                  </Button>
                </Link>
              </div>
            </div>
          </div>
        </header>

        {/* Main Content */}
        <main className="mx-auto max-w-7xl px-4 py-12 sm:px-6 lg:px-8">
          <div className="text-center mb-12">
            <h1 className="font-display text-4xl font-bold tracking-tight sm:text-5xl md:text-6xl mb-6">
              <span className="block">Welcome back, {user.name?.split(" ")[0] || "User"}!</span>
            </h1>
            <p className="text-xl text-muted-foreground max-w-3xl mx-auto">
              You are signed in to the FSH Platform. Here's your profile information and quick access to your resources.
            </p>
          </div>

          {/* Profile Info Card */}
          <Card className="max-w-2xl mx-auto mb-12">
            <CardHeader>
              <CardTitle>Your Profile</CardTitle>
              <CardDescription>Account information and settings</CardDescription>
            </CardHeader>
            <CardContent className="space-y-4">
              <div className="flex items-center gap-4">
                <div className="grid size-12 shrink-0 place-items-center rounded-full bg-primary/10 text-primary font-display text-lg">
                  {(user.name?.[0] || user.email?.[0] || "?").toUpperCase()}
                </div>
                <div>
                  <p className="font-semibold">{user.name || "No name set"}</p>
                  <p className="text-sm text-muted-foreground">{user.email}</p>
                  {user.tenant && (
                    <p className="text-xs text-muted-foreground">Tenant: {user.tenant}</p>
                  )}
                </div>
              </div>
              <Link to="/settings/profile">
                <Button variant="outline" size="sm">
                  Edit Profile
                </Button>
              </Link>
            </CardContent>
          </Card>

          {/* Quick Actions */}
          <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6 mb-12">
            <Card>
              <CardHeader>
                <CardTitle className="flex items-center gap-2">
                  <Shield className="h-5 w-5 text-primary" />
                  Security
                </CardTitle>
              </CardHeader>
              <CardContent>
                <CardDescription>
                  Manage your password, two-factor authentication, and security settings.
                </CardDescription>
              </CardContent>
            </Card>

            <Card>
              <CardHeader>
                <CardTitle className="flex items-center gap-2">
                  <Users className="h-5 w-5 text-primary" />
                  Team
                </CardTitle>
              </CardHeader>
              <CardContent>
                <CardDescription>
                  View and manage your team members and roles.
                </CardDescription>
              </CardContent>
            </Card>

            <Card>
              <CardHeader>
                <CardTitle className="flex items-center gap-2">
                  <BookOpen className="h-5 w-5 text-primary" />
                  Settings
                </CardTitle>
              </CardHeader>
              <CardContent>
                <CardDescription>
                  Customize your preferences and appearance.
                </CardDescription>
              </CardContent>
            </Card>
          </div>

          {/* Quick Links */}
          <Card className="max-w-3xl mx-auto">
            <CardHeader>
              <CardTitle>Quick Links</CardTitle>
              <CardDescription>Access your workspace quickly</CardDescription>
            </CardHeader>
            <CardContent>
              <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
                <Link
                  to="/activity"
                  className="flex items-center justify-between p-3 rounded-lg border hover:bg-accent transition-colors"
                >
                  <div className="flex items-center gap-3">
                    <CheckCircle2 className="h-5 w-5 text-muted-foreground" />
                    <span>Activity</span>
                  </div>
                  <ArrowRight className="h-4 w-4 text-muted-foreground" />
                </Link>
                <Link
                  to="/catalog/products"
                  className="flex items-center justify-between p-3 rounded-lg border hover:bg-accent transition-colors"
                >
                  <div className="flex items-center gap-3">
                    <CheckCircle2 className="h-5 w-5 text-muted-foreground" />
                    <span>Catalog</span>
                  </div>
                  <ArrowRight className="h-4 w-4 text-muted-foreground" />
                </Link>
                <Link
                  to="/subscription"
                  className="flex items-center justify-between p-3 rounded-lg border hover:bg-accent transition-colors"
                >
                  <div className="flex items-center gap-3">
                    <CheckCircle2 className="h-5 w-5 text-muted-foreground" />
                    <span>Subscription</span>
                  </div>
                  <ArrowRight className="h-4 w-4 text-muted-foreground" />
                </Link>
                <Link
                  to="/identity/users"
                  className="flex items-center justify-between p-3 rounded-lg border hover:bg-accent transition-colors"
                >
                  <div className="flex items-center gap-3">
                    <CheckCircle2 className="h-5 w-5 text-muted-foreground" />
                    <span>Team Management</span>
                  </div>
                  <ArrowRight className="h-4 w-4 text-muted-foreground" />
                </Link>
              </div>
            </CardContent>
          </Card>
        </main>

        {/* Footer */}
        <footer className="border-t border-border py-8 text-center text-muted-foreground">
          <p className="text-sm">
            &copy; {new Date().getFullYear()} FSH Platform. All rights reserved.
          </p>
        </footer>
      </div>
    );
  }

  // Public view - no authentication
  return (
    <div className="min-h-screen bg-gradient-to-b from-background via-background to-accent">
      {/* Header */}
      <header className="border-b border-border bg-card/50 backdrop-blur supports-[backdrop-filter]:bg-card/50">
        <div className="mx-auto max-w-7xl px-4 py-6 sm:px-6 lg:px-8">
          <div className="flex items-center justify-between">
            <div className="flex items-center gap-2">
              <Globe className="h-6 w-6 text-primary" />
              <span className="font-display text-xl font-bold">FSH Platform</span>
            </div>
          </div>
        </div>
      </header>

      {/* Main Content */}
      <main className="mx-auto max-w-7xl px-4 py-12 sm:px-6 lg:px-8">
        <div className="text-center mb-12">
          <h1 className="font-display text-4xl font-bold tracking-tight sm:text-5xl md:text-6xl mb-6">
            <span className="block">Welcome to the Platform</span>
            <span className="block text-primary">Your starting point</span>
          </h1>
          <p className="text-xl text-muted-foreground max-w-3xl mx-auto">
            A modern, modular platform for building enterprise SaaS applications.
            Explore the features and get started with your workflow.
          </p>
        </div>

        {/* Features Grid */}
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6 mb-12">
          <Card>
            <CardHeader>
              <CardTitle className="flex items-center gap-2">
                <Shield className="h-5 w-5 text-primary" />
                Security First
              </CardTitle>
            </CardHeader>
            <CardContent>
              <CardDescription>
                Enterprise-grade security with JWT authentication, role-based access control,
                and audit logging.
              </CardDescription>
            </CardContent>
          </Card>

          <Card>
            <CardHeader>
              <CardTitle className="flex items-center gap-2">
                <Users className="h-5 w-5 text-primary" />
                Team Collaboration
              </CardTitle>
            </CardHeader>
            <CardContent>
              <CardDescription>
                Built-in user management, role assignment, and group permissions
                for seamless team workflows.
              </CardDescription>
            </CardContent>
          </Card>

          <Card>
            <CardHeader>
              <CardTitle className="flex items-center gap-2">
                <BookOpen className="h-5 w-5 text-primary" />
                Modular Architecture
              </CardTitle>
            </CardHeader>
            <CardContent>
              <CardDescription>
                Extensible modules for billing, tickets, chat, files, and webhooks.
                Add features as your business grows.
              </CardDescription>
            </CardContent>
          </Card>
        </div>

        {/* Action Buttons */}
        <div className="flex flex-col sm:flex-row gap-4 justify-center mb-12">
          <Button asChild size="lg" className="h-12 px-6">
            <Link to="/login">
              Sign In
              <ArrowRight className="ml-2 h-4 w-4" />
            </Link>
          </Button>
          <Button asChild variant="outline" size="lg" className="h-12 px-6">
            <Link to="/register">
              Create Account
            </Link>
          </Button>
        </div>

        {/* Quick Links */}
        <Card className="max-w-3xl mx-auto">
          <CardHeader>
            <CardTitle>Quick Links</CardTitle>
            <CardDescription>Explore the platform features</CardDescription>
          </CardHeader>
          <CardContent>
            <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
              <Link
                to="/login"
                className="flex items-center justify-between p-3 rounded-lg border hover:bg-accent transition-colors"
              >
                <div className="flex items-center gap-3">
                  <CheckCircle2 className="h-5 w-5 text-muted-foreground" />
                  <span>Dashboard</span>
                </div>
                <ArrowRight className="h-4 w-4 text-muted-foreground" />
              </Link>
              <Link
                to="/login"
                className="flex items-center justify-between p-3 rounded-lg border hover:bg-accent transition-colors"
              >
                <div className="flex items-center gap-3">
                  <CheckCircle2 className="h-5 w-5 text-muted-foreground" />
                  <span>API Documentation</span>
                </div>
                <ArrowRight className="h-4 w-4 text-muted-foreground" />
              </Link>
              <Link
                to="/login"
                className="flex items-center justify-between p-3 rounded-lg border hover:bg-accent transition-colors"
              >
                <div className="flex items-center gap-3">
                  <CheckCircle2 className="h-5 w-5 text-muted-foreground" />
                  <span>Support Center</span>
                </div>
                <ArrowRight className="h-4 w-4 text-muted-foreground" />
              </Link>
              <Link
                to="/login"
                className="flex items-center justify-between p-3 rounded-lg border hover:bg-accent transition-colors"
              >
                <div className="flex items-center gap-3">
                  <CheckCircle2 className="h-5 w-5 text-muted-foreground" />
                  <span>Pricing</span>
                </div>
                <ArrowRight className="h-4 w-4 text-muted-foreground" />
              </Link>
            </div>
          </CardContent>
        </Card>
      </main>

      {/* Footer */}
      <footer className="border-t border-border py-8 text-center text-muted-foreground">
        <p className="text-sm">
          &copy; {new Date().getFullYear()} FSH Platform. All rights reserved.
        </p>
      </footer>
    </div>
  );
}