using FSH.Framework.Web.Mod;
using System.Runtime.CompilerServices;

[assembly: FshModule(typeof(FSH.Mod.Identity.IdentityModule), 150)]
[assembly: InternalsVisibleTo("Identity.Tests")]