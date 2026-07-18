using FSH.Mods.Billing.Spec.v1;

namespace FSH.Mods.Billing.Services;

/// <summary>Renders an invoice to a self-contained PDF document (on-demand, no stored artifact).</summary>
public interface IInvoicePdfRenderer
{
    byte[] Render(InvoiceDto invoice);
}