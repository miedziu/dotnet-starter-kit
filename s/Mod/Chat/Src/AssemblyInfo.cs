using FSH.Framework.Web.Mod;
using System.Runtime.CompilerServices;

[assembly: FshModule(typeof(FSH.Mod.Chat.ChatModule), 400)]
[assembly: InternalsVisibleTo("Chat.Tests")]
[assembly: InternalsVisibleTo("Integration.Tests")]