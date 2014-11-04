function GameConnection::saveMiningGame(%this,%reset) {
	%filename = "config/server/Mining/saves/" @ %this.bl_id;
	%file = new FileObject();
	%file.openForWrite(%filename);

	if(%reset $= "reset") {
		%this.level = 1;
		%this.cash = 0;
		%this.points = 0;
		%this.autoClickDelay = 400;
		%this.dirt = 0;
		%this.maxHealth = 100;
		%this.checkpoint = "20 20 50000";
		%this.rank = 0;
		%this.updateMiningHUD();
	}
	%file.writeLine("level" TAB %this.level);
	%file.writeLine("cash" TAB %this.cash);
	%file.writeLine("points" TAB %this.points);
	%file.writeLine("autoClickDelay" TAB %this.autoClickDelay);
	%file.writeLine("dirt" TAB %this.dirt);
	%file.writeLine("maxHealth" TAB %this.maxHealth);
	%file.writeLine("checkpoint" TAB %this.checkpoint);
	%file.writeLine("rank" TAB %this.rank);

	%file.close();
	%file.delete();

	messageClient(%this,'',"\c4INFO: \c6Your game has been saved.");
}

function GameConnection::loadMiningGame(%this) {
	%filename = "config/server/Mining/saves/" @ %this.bl_id;
	if(!isFile(%filename)) {
		%this.level = 1;
		%this.cash = 0;
		%this.points = 0;
		%this.autoClickDelay = 400;
		%this.dirt = 0;
		%this.maxHealth = 100;
		%this.checkpoint = "20 20 50000";
		%this.rank = 0;
	}
	else {
		%file = new FileObject();
		%file.openForRead(%filename);

		// trying this method of loading, usually it'd look like the saving function.
		while(!%file.isEOF()) {
			%line = %file.readLine();
			eval("%this." @ getField(%line,0) @ "=" @ getField(%line,1) @ ";");
		}

		%file.close();
		%file.delete();
	}
	%this.score = %this.level;
}

function GameConnection::miningSaveLoop(%this) {
	if(%this.saveLoop)
		cancel(%this.saveLoop);
	%this.saveLoop = %this.schedule(60000,miningSaveLoop);
	%this.saveMiningGame();
}