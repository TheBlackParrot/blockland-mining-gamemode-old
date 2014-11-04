function serverCmdUpgrade(%client,%amount) {
	if(!%amount)
		%amount = 1;

	for(%i=0;%i<%amount;%i++) {
		if(%client.getUpgradeCost("pick") > %client.cash) {
			messageClient(%client,'',"\c0ERROR:\c6 You do not have enough to upgrade your pickaxe to level" SPC %client.level);
			return;
		}
		else {
			%sound = 1;
			%client.cash = Math_Subtract(%client.cash,%client.getUpgradeCost("pick"));
			%client.level++;
			%client.updateMiningHUD();
			messageClient(%client,'',"\c4INFO:\c6 You have leveled up your pickaxe to level" SPC %client.level);
		}
	}
	if(%sound)
		%client.play2D(miningLevelSound);
		
	%client.score = %client.level;
}
function serverCmdUpgradePick(%client,%amount) { serverCmdUpgrade(%client,%amount); }

function serverCmdAutoclick(%client,%status) {
	%player = %client.player;
	switch$(%status) {
		case "on":
			if(%player.autoClickEnabled)
				return;
			else {
				%player.autoClick();
				%player.autoClickEnabled = 1;
				messageClient(%client,'',"\c4INFO:\c6 Autoclick has been enabled. To upgrade it, use /upgradeautoclick [amount].");
			}
		case "off":
			if(!%player.autoClickEnabled)
				return;
			else {
				cancel(%player.autoClick);
				%player.autoClickEnabled = 0;
				messageClient(%client,'',"\c4INFO:\c6 Autoclick has been disabled.");
			}
	}
}

function serverCmdUpgradeAutoClick(%client,%amount) {
	if(!%amount)
		%amount = 1;
	if(%client.autoClickDelay <= 130) {
		messageClient(%client,'',"\c0ERROR:\c6130ms is the lowest delay for the autoclicker.");
		return;
	}

	for(%i=0;%i<%amount;%i++) {
		if(%client.autoClickDelay <= 130) {
			messageClient(%client,'',"\c0ERROR:\c6130ms is the lowest delay for the autoclicker.");
			return;
		}
		if(%client.getUpgradeCost("click") > %client.cash) {
			messageClient(%client,'',"\c0ERROR:\c6 You need <color:00aa00>$" @ %client.getUpgradeCost("click") SPC "\c6to decrease your autoclick delay to" SPC %client.autoClickDelay-2);
			return;
		}
		else {
			%sound = 1;
			%client.cash = Math_Subtract(%client.cash,%client.getUpgradeCost("click"));
			%client.autoClickDelay -= 2;
			%client.updateMiningHUD();
			messageClient(%client,'',"\c4INFO:\c6 You have decreased your autoclick delay to" SPC %client.autoClickDelay);
		}
	}
	if(%sound)
		%client.play2D(miningLevelSound);
}

function serverCmdSaveGame(%client) { %client.saveMiningGame(); }
function serverCmdLoadGame(%client) { %client.loadMiningGame(); }

function serverCmdResetGame(%client,%is_sure) {
	if(%is_sure $= "") {
		%client.centerPrint("\c6If you are \c0ABSOLUTELY SURE \c6that you want to reset your stats, please type \c5/resetgame iamsure<br>You will also lose your saved game.");
		return;
	}
	if(%is_sure $= "iamsure") {
		%client.saveMiningGame("reset");
		%client.centerPrint("Your game has been reset. You will respawn in 5 seconds.");
		%client.player.schedule(5000,instantRespawn);
	}
}

function serverCmdRespawn(%client) {
	if(getSimTime() - %client.lastRespawned >= 5000) {
		%client.player.instantRespawn();
		%client.lastRespawned = getSimTime();
	}
}

function serverCmdHeal(%client,%amount) {
	if(!%amount)
		%amount = %client.maxHealth - %client.player.health;
	if(!%amount)
		return;

	for(%i=0;%i<%amount;%i++) {
		if(%client.player.health >= %client.maxHealth)
			return;
		if(%client.dirt < 5) {
			messageClient(%client,'',"\c0ERROR:\c6 You need <color:99aa00>5 dirt \c6to heal.");
			return;
		}
		else {
			%sound = 1;
			%client.dirt = Math_Subtract(%client.dirt,5);
			%client.player.health += 1;
			%client.updateMiningHUD();
		}
	}
	if(%sound)
		%client.play2D(miningLevelSound);
}

function serverCmdUpgradeMaxHealth(%client) {
	if(!%amount)
		%amount = 1;

	for(%i=0;%i<%amount;%i++) {
		if(%client.getUpgradeCost("health") > %client.cash) {
			messageClient(%client,'',"\c0ERROR:\c6 You need <color:00aa00>$" @ %client.getUpgradeCost("health") SPC "\c6to increase your maximum health to" SPC %client.maxHealth+5);
			return;
		}
		else {
			%sound = 1;
			%client.cash = Math_Subtract(%client.cash,%client.getUpgradeCost("health"));
			%client.maxHealth += 5;
			%client.updateMiningHUD();
			messageClient(%client,'',"\c4INFO:\c6 You have increased your maximum health to" SPC %client.maxHealth);
		}
	}
	if(%sound)
		%client.play2D(miningLevelSound);
}

function serverCmdSetCheckpoint(%client) {
	%player = %client.player;
	%pos = %player.getPosition();
	%x = mFloor(getWord(%pos,0)) - (mFloor(getWord(%pos,0)) % 4);
	%y = mFloor(getWord(%pos,1)) - (mFloor(getWord(%pos,1)) % 4);
	%z = mFloor(getWord(%pos,2)) - (mFloor(getWord(%pos,2)) % 4);

	%client.checkpoint = %x SPC %y SPC %z;
	messageClient(%client,'',"\c4INFO: \c6You have set your checkpoint to" SPC %x/4 @ "x" SPC %y/4 @ "y" SPC %z/4 @ "z");
}

function serverCmdGoToCheckpoint(%client) {
	%player = %client.player;
	%dist = mFloor(vectorDist(%player.getPosition(),%client.checkpoint));

	if(mFloor(%dist/8) > %client.cash) {
		messageClient(%client,'',"\c0ERROR:\c6 You need <color:00aa00>$" @ mFloor(%dist/8) SPC "\c6to return to your checkpoint.");
		return;
	}
	else {
		if(!$Mining::Brick[getWord(%client.checkpoint,0),getWord(%client.checkpoint,1),getWord(%client.checkpoint,2)]) {
			Mining_placeBrick(getWord(%client.checkpoint,0),getWord(%client.checkpoint,1),getWord(%client.checkpoint,2),1);
			Mining_doExplosion(%client.checkpoint,5);
		}
		%client.cash = Math_Subtract(%client.cash,mFloor(%dist/8));
		%client.player.setTransform(getWord(%client.checkpoint,0) SPC getWord(%client.checkpoint,1) SPC getWord(%client.checkpoint,2)+2);
		%client.updateMiningHUD();
	}	
}

function serverCmdHelp(%client) {
	messageClient(%client,'',"\c2COMMANDS:");
	messageClient(%client,'',"\c3/upgrade \c4[amount or empty]\c6 -- Upgrades your pick level by [amount] or 1.");
	messageClient(%client,'',"\c3/autoclick \c4[on,off]\c6 -- Turns on or off the autoclicker.");
	messageClient(%client,'',"\c3/upgradeautoclick \c4[times or empty] \c6-- Decreases your autoclick delay by 2ms; can be at most 130ms.");
	messageClient(%client,'',"\c3/heal \c4[amount or empty] \c6-- Heals you by [amount] or all the way.");
	messageClient(%client,'',"\c3/upgrademaxhealth \c4[times or empty] \c6-- Increases your maximum health by 5.");
	messageClient(%client,'',"\c3/respawn \c6-- Respawns you.");
	messageClient(%client,'',"\c3/reset \c6-- Resets your \c0entire game.");
	messageClient(%client,'',"\c3/savegame \c6-- Manually save your game.");
	messageClient(%client,'',"\c3/loadgame \c6-- Manually load your most recent saved game.");
	messageClient(%client,'',"\c3/setcheckpoint \c6-- Sets your checkpoint at your current position.");
	messageClient(%client,'',"\c3/gotocheckpoint \c6-- Teleports you to your checkpoint. Cost depends on the distance from your current position.");
	messageClient(%client,'',"\c4Use Page Up/Page Down to navigate the help.");
	messageClient(%client,'',"\c5[Please note this gamemode is heavily under development, and things may be added (a lot), changed, or removed at any time.]");
}