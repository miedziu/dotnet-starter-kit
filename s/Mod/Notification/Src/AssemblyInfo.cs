using FSH.Framework.Web.Mod;
using System.Runtime.CompilerServices;

[assembly: FshModule(typeof(FSH.Mod.Notification.NotificationModule), 500)]
[assembly: InternalsVisibleTo("Notification.Tests")]
[assembly: InternalsVisibleTo("Integration.Tests")]