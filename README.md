# UniversalModeration
- Id: Feli.UniversalModeration
- Description: An universal moderation plugin for openmod that will allow you to manage your server
- Version: 1.0.5

## Commands
- warns /warns userId: A command to check the warns of a user
  id: UniversalModeration.Commands.WarnsCommand
- warn /warn userName Optional: (reason): A command to warn a user
  id: UniversalModeration.Commands.WarnCommand
- unban /unban userId: A command to unban a user
  id: UniversalModeration.Commands.UnBanCommand
- kick /kick userName Optional: (reason): A command to kick a user
  id: UniversalModeration.Commands.KickCommand
- bans /bans userId: A command to check the bans of a user
  id: UniversalModeration.Commands.BansCommand
- ban /ban userName Optional: (reason time): A command to ban users
  id: UniversalModeration.Commands.BanCommand

## Permissions
- Feli.UniversalModeration:commands.ban: Grants access to the UniversalModeration.Commands.BanCommand command.
- Feli.UniversalModeration:commands.bans: Grants access to the UniversalModeration.Commands.BansCommand command.
- Feli.UniversalModeration:commands.kick: Grants access to the UniversalModeration.Commands.KickCommand command.
- Feli.UniversalModeration:commands.unban: Grants access to the UniversalModeration.Commands.UnBanCommand command.
- Feli.UniversalModeration:commands.warn: Grants access to the UniversalModeration.Commands.WarnCommand command.
- Feli.UniversalModeration:commands.warns: Grants access to the UniversalModeration.Commands.WarnsCommand command.
