using FSH.Framework.Web.Modules;
using System.Runtime.CompilerServices;

[assembly: FshModule(typeof(FSH.Mods.Identity.IdentityModule), 150)]
[assembly: InternalsVisibleTo("Identity.Tests")]