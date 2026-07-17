using FSH.Framework.Web.Modules;
using System.Runtime.CompilerServices;

[assembly: FshModule(typeof(FSH.Modules.Chat.ChatModule), 400)]
[assembly: InternalsVisibleTo("Chat.Tests")]
[assembly: InternalsVisibleTo("Integration.Tests")]