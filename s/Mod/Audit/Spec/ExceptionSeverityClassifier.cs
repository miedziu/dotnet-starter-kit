namespace FSH.Mod.Audit.Spec;

public static class ExceptionSeverityClassifier
{
    public static AuditSeverity Classify(Exception ex) =>
        ex switch
        {
            OperationCanceledException => AuditSeverity.Information,
            UnauthorizedAccessException => AuditSeverity.Warning,
            _ => AuditSeverity.Error
        };
}