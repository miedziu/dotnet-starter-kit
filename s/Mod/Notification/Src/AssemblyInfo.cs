using FSH.Framework.Web.Modules;
using System.Runtime.CompilerServices;

[assembly: FshModule(typeof(FSH.Modules.Notifications.NotificationsModule), 500)]
[assembly: InternalsVisibleTo("Notifications.Tests")]
[assembly: InternalsVisibleTo("Integration.Tests")]