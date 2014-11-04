$Mining::Root = "Add-Ons/Gamemode_Mining/";
$Mining::BaseHeight = 50000;

PlayerStandardArmor.jumpForce = 1250;

if($Mining::HasInitiated) {
	for(%i=0;%i<Mining_DirtDatabase.getCount();%i++)
		Mining_DirtDatabase.getObject(%i).delete();
	Mining_DirtDatabase.delete();

	for(%i=0;%i<Mining_OreDatabase.getCount();%i++)
		Mining_OreDatabase.getObject(%i).delete();
	Mining_OreDatabase.delete();

	MiningAI.delete();
}
else {
	schedule(3000,0,Mining_newSpawn);
	$Mining::ChanceBuff = 1;
	$Mining::HasInitiated = 1;
}

new SimGroup(Mining_DirtDatabase) {
	initiated = getDateTime();
};
new SimGroup(Mining_OreDatabase) {
	initiated = getDateTime();
};
new SimGroup(Mining_SpecialDatabase) {
	initiated = getDateTime();
};

new AIConnection(MiningAI)
{
	isAdmin = 1;
	isSuperAdmin = 1;
	bl_id = 999998;
};

function Mining_initDirtDatabase() {
	%filename = $Mining::Root @ "db/dirt.db";
	%file = new FileObject();
	%file.openForRead(%filename);

	while(!%file.isEOF()) {
		%line = %file.readLine();
		if(getSubStr(%line,0,2) $= "//")
			continue;
		%row = new ScriptObject(DirtDatabaseRow) {
			name = getField(%line,0);
			health = getField(%line,1);
			max_health = getField(%line,1);
			color = getField(%line,2);
			min_z = getField(%line,3);
			max_z = getField(%line,4);
			added = getDateTime();
		};
		Mining_DirtDatabase.add(%row);
	}

	%file.close();
	%file.delete();
}
Mining_initDirtDatabase();

function Mining_initOreDatabase() {
	%filename = $Mining::Root @ "db/ores.db";
	%file = new FileObject();
	%file.openForRead(%filename);

	while(!%file.isEOF()) {
		%line = %file.readLine();
		if(getSubStr(%line,0,2) $= "//")
			continue;
		%row = new ScriptObject(OreDatabaseRow) {
			name = getField(%line,0);
			health = getField(%line,1);
			max_health = getField(%line,1);
			value = getField(%line,2);
			max_level = getField(%line,3);
			color = getField(%line,4);
			chance = getField(%line,5);
			min_z = getField(%line,6);
			max_z = getField(%line,7);
			colorfx = getField(%line,8);
			demandBuff = 1;
			added = getDateTime();
		};
		Mining_OreDatabase.add(%row);
	}

	%file.close();
	%file.delete();
}
Mining_initOreDatabase();

function Mining_initSpecialDatabase() {
	%health = getRandom(500,1500);
	%row = new ScriptObject(SpecialDatabaseRow) {
		name = "Chance Block";
		health = %health;
		max_level = mFloor(%health/20);
		color = 27;
		colorFx = 3;
		added = getDateTime();
	};
	Mining_SpecialDatabase.add(%row);

	%row = new ScriptObject(SpecialDatabaseRow) {
		name = "Lava";
		health = 0;
		max_level = 0;
		color = 26;
		colorFx = 3;
		added = getDateTime();
	};
	Mining_SpecialDatabase.add(%row);

	%health = getRandom(100,300);
	%row = new ScriptObject(SpecialDatabaseRow) {
		name = "Dormant Bomb";
		health = %health;
		max_level = mFloor(%health/20);
		color = 29;
		colorFx = 3;
		added = getDateTime();
	};
	Mining_SpecialDatabase.add(%row);
}
Mining_initSpecialDatabase();

function Mining_newSpawn()
{
	for(%i=0;%i<BrickGroup_888888.getCount();%i++)
		if(BrickGroup_888888.getObject(%i).getClassName() $= "fxDTSBrick")
			BrickGroup_888888.getObject(%i).killBrick();
	deleteVariables("$Mining::Brick*");

	%base = 400;
	for(%x=0;%x<12;%x++)
		for(%y=0;%y<12;%y++)
			for(%z=0;%z<12;%z++)
				Mining_placeBrick(%x*4,%y*4,(%z*4)+$Mining::BaseHeight,1);
	for(%x=0;%x<10;%x++)
		for(%y=0;%y<10;%y++)
			for(%z=0;%z<10;%z++)
				$Mining::Brick[4+(%x*4),4+(%y*4),(%z*4)+$Mining::BaseHeight+4].delete();

	PlayerDropPoints.delete();
	%points = new SimGroup(PlayerDropPoints) {
		new SpawnSphere() {
			position = "20 20" SPC $Mining::BaseHeight-120;
			rotation = "0 0 1 130.062";
			scale = "0.940827 1.97505 1";
			dataBlock = "SpawnSphereMarker";
			canSetIFLs = "0";
			radius = "20";
			sphereWeight = "1";
			indoorWeight = "1";
			outdoorWeight = "1";
			RayHeight = "150";
		};
	};
	MissionGroup.add(%points);

	$DefaultMinigame.economyLoop();
	$DefaultMinigame.demandBuffLoop();
	$DefaultMinigame.dropLoop();
	$DefaultMinigame.ambianceLoop();
	$DefaultMinigame.regeneratePlayers();
	$DefaultMinigame.checkBrickcount();

	for(%i=0;%i<$DefaultMinigame.numMembers;%i++) {
		%client = $DefaultMinigame.member[%i];
		if(!isObject(%client.player))
			%client.spawnPlayer();
		else
			%client.player.instantRespawn();
	}
}

function Mining_initPoemDatabase() {
	%filename = $Mining::Root @ "db/poems.db";
	%file = new FileObject();
	%file.openForRead(%filename);

	%i=-1;
	%j=0;
	while(!%file.isEOF()) {
		%line = %file.readLine();
		if(getSubStr(%line,0,2) $= "--") {
			%line = getSubStr(%line,2,strLen(%line));
			%i++;
			%j=0;
		}
		%j++;
		$Mining::Poem[%i,%j] = %line;
		$Mining::Poem_LC[%i] = %j;
		$Mining::PoemCount = %i;
	}

	$Mining::Poem["phrase",0] = "death";
	$Mining::Poem["phrase",1] = "life";
	$Mining::Poem["phrase",2] = "is there any meaning";
	$Mining::Poem["phrase",3] = "that sounds nice";
	$Mining::Poem["phrase",4] = "d r i p    d r o p";
	$Mining::Poem["phrase",5] = "frozen in time";
	$Mining::Poem["phrase",6] = "still the best 1972";
	$Mining::Poem["phrase",7] = "slow down";
	$Mining::Poem["phrase",8] = "this is all a lie";
	$Mining::Poem["phrase",9] = "emptiness";
	$Mining::Poem["phrase",10] = "what ONCE WAS";
	$Mining::Poem["phrase",11] = "NAGARI: 999";
	$Mining::Poem["phrase",12] = "blue planet out in the free of space";
	$Mining::Poem["phrase",13] = "take me away";
	$Mining::Poem["phrase",14] = "Aluminum";
	$Mining::Poem["phrase",15] = "Nickel";
	$Mining::Poem["phrase",16] = "look beh i n d you";
	$Mining::Poem["phrase",17] = "smile ):";
	$Mining::Poem["phrase",18] = "brain rot";
	$Mining::Poem["phrase",19] = "HELP me not, fools";
	$Mining::Poem["phrase",20] = "missing";

	%file.close();
	%file.delete();
}
Mining_initPoemDatabase();