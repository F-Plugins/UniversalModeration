# UniversalModeration
- Id: Feli.UniversalModeration
- Description: An universal moderation plugin for openmod that will allow you to manage your server
- Version: 1.0.3

## Commands
- unban /unban userId: A command to unban a user
  id: UniversalModeration.Commands.UnBanCommand
- kick /kick userName Optional: (reason): A command to kick a user
  id: UniversalModeration.Commands.KickCommand
- bans /bans userId: A command to check the bans of a user
  id: UniversalModeration.Commands.BansCommand
- ban /ban userName Optional: (reason> time): A command to ban users
  id: UniversalModeration.Commands.BanCommand

## Permissions
- Feli.UniversalModeration:commands.ban: Grants access to the UniversalModeration.Commands.BanCommand command.
- Feli.UniversalModeration:commands.bans: Grants access to the UniversalModeration.Commands.BansCommand command.
- Feli.UniversalModeration:commands.kick: Grants access to the UniversalModeration.Commands.KickCommand command.
- Feli.UniversalModeration:commands.unban: Grants access to the UniversalModeration.Commands.UnBanCommand command.
