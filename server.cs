exec("./functions.cs");
exec("./init.cs");
exec("./interaction.cs");
exec("./saving.cs");
exec("./math.cs");
exec("./ambiance.cs");
exec("./explosion.cs");
exec("./health.cs");
exec("./poems.cs");
exec("./bside.cs");
$Mining::Version = "0.18.1.3";

datablock AudioProfile(miningTapSound)
{
	filename = "./sound/tap.wav";
	description = AudioClosest3d;
	preload = true;
};
datablock AudioProfile(miningPopSound)
{
	filename = "./sound/pop.wav";
	description = AudioClosest3d;
	preload = true;
};
datablock AudioProfile(miningCashSound)
{
	filename = "./sound/cash.wav";
	description = AudioClosest3d;
	preload = true;
};
datablock AudioProfile(miningLevelSound)
{
	filename = "./sound/levelup2.wav";
	description = AudioClosest3d;
	preload = true;
};

// ++++++++++++++++++++++++++++
// + HEAVILY WORK IN PROGRESS +
// +  THIS IS LITERALLY NOT   +
// +   VERY FUN YET, DON'T    +
// +  EXPECT VERY MUCH YET.   +
// ++++++++++++++++++++++++++++

package MiningPackage {
	function Player::ActivateStuff(%this) {
		if(!%this.autoClickEnabled)
			%this.mineBlock();
		return parent::ActivateStuff(%this);
	}
	function GameConnection::autoAdminCheck(%this) {
		%this.loadMiningGame();
		%this.miningSaveLoop();
		messageClient(%this,'',"\c5BE SURE TO DOWNLOAD SOUNDS!");
		return parent::autoAdminCheck(%this);
	}
	function GameConnection::OnClientLeaveGame(%this) {
		%this.saveMiningGame();
		return parent::OnClientLeaveGame(%this);
	}
	function GameConnection::spawnPlayer(%this) {
		parent::spawnPlayer(%this);
		if(!%this.maxHealth)
			%this.maxHealth = 100;
		if(!%this.player.health)
			%this.player.health = %this.maxHealth;
	}

	function serverCmdLight(%client) {
		if($Mining::BSideEnabled)
			return;
		else
			return parent::serverCmdLight(%client);
	}

	function serverCmdMessageSent(%client,%msg) {
		%old_pre = %client.clanPrefix;
		%client.clanPrefix = "<bitmap:base/client/ui/CI/star.png>\c2" @ %client.rank SPC "\c7" @ %old_pre;
		parent::serverCmdMessageSent(%client,%msg);
		%client.clanPrefix = %old_pre;
	}
};
activatePackage(MiningPackage);

talk("Executed Mining v" @ $Mining::Version);