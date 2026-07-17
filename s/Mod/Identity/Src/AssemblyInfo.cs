using FSH.Framework.Web.Modules;
using System.Runtime.CompilerServices;

[assembly: FshModule(typeof(FSH.Modules.Identity.IdentityModule), 150)]
[assembly: InternalsVisibleTo("Identity.Tests")]