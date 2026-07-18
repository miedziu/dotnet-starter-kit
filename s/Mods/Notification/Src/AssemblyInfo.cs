using FSH.Framework.Web.Modules;
using System.Runtime.CompilerServices;

[assembly: FshModule(typeof(FSH.Mods.Notification.NotificationModule), 500)]
[assembly: InternalsVisibleTo("Notification.Tests")]
[assembly: InternalsVisibleTo("Integration.Tests")]