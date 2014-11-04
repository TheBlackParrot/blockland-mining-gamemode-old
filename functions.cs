function Mining_getBrickType(%z,%force_dirt,%ore) {
	if(%z <= 4)
		return Mining_DirtDatabase.getObject(0);
	if(!%force_dirt) {
		if(!getRandom(0,7) && %ore)
			return %ore;
		if(!getRandom(0,12)) {
			if(%z < 5000)
				%z_scaled = 5000;
			else
				%z_scaled = %z;
			if(!getRandom(0,mFloor(%z_scaled/1750)))
				return Mining_SpecialDatabase.getObject(1);

			if(!getRandom(0,350))
				return Mining_SpecialDatabase.getObject(2);
		}
		if(!getRandom(0,30)) {
			if(!getRandom(0,100) && !$Mining::ChanceBlockInEffect)
				return Mining_SpecialDatabase.getObject(0);
			
			%j = 0;
			for(%i=0;%i<Mining_OreDatabase.getCount();%i++) {
				%row = Mining_OreDatabase.getObject(%i);
				if(%z >= %row.min_z && %z <= %row.max_z) {
					%ore[%j] = %row;
					%total += %row.chance;
					%ore_low[%j] = %total - %row.chance;
					%ore_hi[%j] = %ore_low[%j] + %row.chance;
					%j++;
				}
			}
			%chance = getRandom(0,%total);

			for(%i=0;%i<%j;%i++)
				if(%chance >= %ore_low[%i] && %chance <= %ore_hi[%i])
					return %ore[%i];
		}
	}
	if(%z == 50000 && %force_dirt)
		return Mining_DirtDatabase.getObject(0);
	for(%i=0;%i<Mining_DirtDatabase.getCount();%i++) {
		%row = Mining_DirtDatabase.getObject(%i);
		if(%z >= %row.min_z && %z <= %row.max_z) {
			return %row;
		}
	}
}

function getZeroVar(%variable) {
	return %variable || 0;
}

function getFormattedNumber(%num,%dec)
{
	//math.cs now required
	if(%dec)
	{
		if(getDecimal(%num) != -1)
		{
			%decimals = getDecPart(%num);
			%num = getSubStr(%num,0,getDecimal(%num));
		}
		if(%dec > strLen(%decimals)) {
			while(strLen(%decimals) != %dec)
				%decimals = %decimals @ "0";
		}
		%decimals = "." @ %decimals;
	}

	%length = strLen(%num);

	if(%length < 4)
		return %num @ %decimals;

	for(%i=%length-3;%i>0;%i-=3)
		%string = "," @ getSubStr(%num,%i,3) @ %string;
	%i += 3;
	%string = getSubStr(%num,0,%i) @ %string;

	return %string @ %decimals;
}

function Mining_placeBrick(%x,%y,%z,%force_dirt,%ore) {
	if($Mining::Brick[%x,%y,%z])
		return;
	//8x cube = 4 units on each side
	%type = Mining_getBrickType(%z,%force_dirt,%ore);
	if(%type.name $= "Cement")
		%permanent = 1;

	if(%type.name $= "Chance Block")
		%type.health = getRandom(250,1750);

	if(%z <= 10000 && !getRandom(0,%z/100)) {
		%rad_buff = getRandom(2,4);
		%shapeFx = 1;
	}
	else
		%rad_buff = 1;

	if(%type.value) {
		%randp = getRandom(0,$Mining::PoemCount);
		%poem = %randp;
		%line = getRandom(0,$Mining::Poem_LC[%randp]);
	}
	else {
		%poem = "phrase";
		%line = getRandom(0,20);
	}
	%health = getZeroVar(%type.health)*%rad_buff;
	%health += getRandom(-1*mFloor(%health/10),mFloor(%health/10));

	%brick = new fxDTSBrick(MiningBrick) {
		angleID = 0;
		client = MiningAI;
		colorFxID = getZeroVar(%type.colorFx);
		colorFx = getZeroVar(%type.colorFx);
		colorID = %type.color;
		dataBlock = "brick8xCubeData";
		isBasePlate = 1;
		isPlanted = 1;
		position = %x SPC %y SPC %z;
		printID = 0;
		scale = "1 1 1";
		shapeFxID = getZeroVar(%shapeFx);
		stackBL_ID = 888888;
		value = getZeroVar(%type.value)*%rad_buff;
		health = %health;
		max_health = %health;
		max_level = mFloor(getZeroVar(%type.max_level)*%rad_buff/2);
		type = %type.name;
		permanent = getZeroVar(%permanent);
		special = getZeroVar(%type.special);
		rad_buff = %rad_buff;
		poem = %poem;
		line = getZeroVar(%line);
	};
	if(%type.value)
		%brick.row_db = %type;
	%brick.plant();
	%brick.setTrusted(1);
	BrickGroup_888888.add(%brick);
	%brick.playSound(miningPopSound);

	$Mining::Brick[%x,%y,%z] = %brick;
}

function Player::mineBlock(%this) {
	%client = %this.client;

	if(getSimTime() - %this.oldHitTime < 120) {
		messageClient(%this.client,'',"\c0WARNING: \c6You are clicking too fast. Don't strain yourself too hard!");
		return;
	}

	%this.playThread(1,activate);

	%eye = vectorScale(%this.getEyeVector(), 8);
	%pos = %this.getEyePoint();
	%mask = $TypeMasks::FxBrickObjectType;
	%hit = firstWord(containerRaycast(%pos, vectorAdd(%pos, %eye), %mask, %this));

	if(!isObject(%hit))
		return;
	if(%hit.getClassName() $= "fxDTSBrick" && !%hit.permanent) {
		if(!%client.level)
			%client.level = 1;
		
		%hit.playSound(miningTapSound);
		%hit.setColorFX(3);
		%hit.schedule(100,setColorFX,%hit.colorFx);

		if(%hit.type $= "Chance Block" && $Mining::ChanceBlockInEffect) {
			%client.centerPrint("\c6A chance block is already in effect, wait until the buff wears off!");
			return;
		}

		if(%client.level < %hit.max_level) {
			%client.centerPrint("\c6You need to be at least level" SPC %hit.max_level SPC "to mine this block.");
			return;
		}

		if(%hit.oldUser $= "") {
			%hit.oldUser = %client.name;
			%hit.lastHitSched = %hit.schedule(15000,Mining_setPublicAccess);
			%hit.lastHit = getSimTime();
		}
		else {
			if(%hit.oldUser !$= %client.name) {
				%client.centerPrint("\c3" @ %hit.oldUser SPC "\c6already has temporary ownership of the block.<br>Please wait" SPC mFloor(((%hit.lastHit+15000) - getSimTime())/1000) SPC "seconds.",3);
				return;
			}
			else
			{
				%hit.oldUser = %client.name;
				if(%hit.lastHitSched)
					cancel(%hit.lastHitSched);
				%hit.lastHitSched = %hit.schedule(15000,Mining_setPublicAccess);
				%hit.lastHit = getSimTime();
			}
		}		

		if($Mining::BSideEnabled) {
			%this.mineBlockB(%hit);
			return;
		}

		%hit.health = Math_Subtract(%hit.health,%client.level);

		%color = getColorIDTable(%hit.colorID);
		%red = getWord(%color,0)*255;
		%green = getWord(%color,1)*255;
		%blue = getWord(%color,2)*255;

		if(%hit.rad_buff >= 2)
			%temp_str = %temp_str @ "<br><color:aa00ff>[Radiated]";

		if(%hit.health <= 0) {
			%hit.health = 0;
			%pos = %hit.getPosition();
			if(%hit.value) {
				%new_value = Math_Multiply(%hit.value,Math_Multiply(Math_Multiply($Mining::GeneralFluctuation,$Mining::ChanceBuff,3),%hit.getOreType().demandBuff,3),2);
				%client.cash = Math_Add(%new_value,%client.cash);
				%temp_str = "<br><color:00aa00>[$" @ getFormattedNumber(%new_value,2) @ "]<br>\c5[x" @ Math_Multiply($Mining::GeneralFluctuation,$Mining::ChanceBuff,3) @ "] \c1[demand x" @ %hit.getOreType().demandBuff @ "]";
				%hit.playSound(miningCashSound);
				%client.points = Math_Add(mFloor(%hit.value/%hit.rad_buff),%client.points);
				%hit.getOreType().demandBuff -= getRandom(1,6)/1000;
			}
			else {
				switch$(%hit.type) {
					case "Chance Block":
						$DefaultMinigame.doChanceBlock(%client);
						%client.points = Math_Add(%client.points,%hit.max_health);
		
					case "Lava":
						%client.cash = Math_Subtract(%client.cash,%client.level*10);
						if(%client.cash < 0)
							%client.cash = 0;
						%this.modifyHealth(getRandom(-75,-35));
						messageClient(%client,'',"Careful! Mining lava hurts.");

					case "Dormant Bomb":
						messageAll('',%client.name SPC "set off a level" SPC mFloor(%hit.max_health/10) SPC "dormant bomb!");
						schedule(1,0,Mining_doExplosion,%pos,mFloor(%hit.max_health/10));
						%client.points = Math_Add(%hit.max_health,%client.points);
		
					default:
						%client.points = Math_Add(1,%client.points);
						%client.dirt = Math_Add(1,%client.dirt);
				}
			}
			%client.centerPrint("<color:" @ convertRGBToHex(%red) @ convertRGBToHex(%green) @ convertRGBToHex(%blue) @ ">" @ %hit.type @ "\c6: 0 /" SPC %hit.max_health @ %temp_str,3);

			Mining_placeBrick(getWord(%pos,0)+4,getWord(%pos,1),getWord(%pos,2),0,%hit.row_db);
			Mining_placeBrick(getWord(%pos,0)-4,getWord(%pos,1),getWord(%pos,2),0,%hit.row_db);
			Mining_placeBrick(getWord(%pos,0),getWord(%pos,1)+4,getWord(%pos,2),0,%hit.row_db);
			Mining_placeBrick(getWord(%pos,0),getWord(%pos,1)-4,getWord(%pos,2),0,%hit.row_db);
			Mining_placeBrick(getWord(%pos,0),getWord(%pos,1),getWord(%pos,2)+4,0,%hit.row_db);
			Mining_placeBrick(getWord(%pos,0),getWord(%pos,1),getWord(%pos,2)-4,0,%hit.row_db);

			%hit.playSound(miningPopSound);
			if(%hit)
				%hit.delete();
		}
		else {
			%client.centerPrint("<color:" @ convertRGBToHex(%red) @ convertRGBToHex(%green) @ convertRGBToHex(%blue) @ ">" @ %hit.type @ "\c6:" SPC %hit.health SPC "/" SPC %hit.max_health @ %temp_str,3);
		}
	}
	%client.updateMiningHUD();
	%this.oldHitTime = getSimTime();
}

function Player::mineBlockB(%this,%hit) {
	%client = %this.client;
		
	%hit.health = Math_Subtract(%hit.health,mCeil(%hit.max_health/4));

	if(%hit.health <= 0) {
		%hit.health = 0;
		%pos = %hit.getPosition();
		if(%hit.value) {
			%hit.playSound(miningLevelSound);
			%client.points = Math_Add(%hit.value,%client.points);
		}
		%client.centerPrint("\c6" @ $Mining::Poem[%hit.poem,%hit.line],3);

		Mining_placeBrick(getWord(%pos,0)+4,getWord(%pos,1),getWord(%pos,2));
		Mining_placeBrick(getWord(%pos,0)-4,getWord(%pos,1),getWord(%pos,2));
		Mining_placeBrick(getWord(%pos,0),getWord(%pos,1)+4,getWord(%pos,2));
		Mining_placeBrick(getWord(%pos,0),getWord(%pos,1)-4,getWord(%pos,2));
		Mining_placeBrick(getWord(%pos,0),getWord(%pos,1),getWord(%pos,2)+4);
		Mining_placeBrick(getWord(%pos,0),getWord(%pos,1),getWord(%pos,2)-4);

		%hit.playSound(miningPopSound);
		if(%hit)
			%hit.delete();
	}
	else {
		%client.centerPrint("\c6" @ encryptPoemLine(%hit.poem,%hit.line,getRandom(1,2)),3);
	}
	%client.updateMiningHUD();
	%this.oldHitTime = getSimTime();
}

function convertRGBToHex(%dec)
{
	%str = "0123456789ABCDEF";

	while(%dec != 0)
	{
		%hexn = %dec % 16;
		%dec = mFloor(%dec / 16);
		%hex = getSubStr(%str,%hexn,1) @ %hex;    
	}

	if(strLen(%hex) == 1)
		%hex = "0" @ %hex;
	if(!strLen(%hex))
		%hex = "00";

	return %hex;
}

function fxDTSBrick::getOreType(%this) {
	%type = strReplace(%this.type,"Radiated ","");
	for(%i=0;%i<Mining_OreDatabase.getCount();%i++)
		if(Mining_OreDatabase.getObject(%i).name $= %type)
			return Mining_OreDatabase.getObject(%i);
}

function fxDTSBrick::Mining_setPublicAccess(%this) {
	%this.lastHit = "";
	%this.oldUser = "";
}

function GameConnection::updateMiningHUD(%this) {
	%pos = %this.player.getPosition();
	%x = mFloor(getWord(%pos,0)/4) @ "x";
	%y = mFloor(getWord(%pos,1)/4) @ "y";
	%z = mFloor(getWord(%pos,2)/4) @ "z";
	%this.bottomPrint("<font:Arial Bold:22>\c2$\c6" @ getFormattedNumber(%this.cash,2) SPC "<font:Arial Bold:16><color:994400>/" SPC getFormattedNumber(%this.dirt) SPC "dirt\c5 /" SPC getFormattedNumber(%this.points) SPC "pts<just:right>\c6LOC:\c4" SPC %x SPC %y SPC %z SPC "\c7:: \c6CR:\c5" SPC getFormattedNumber(getSimTime() - %this.player.oldHitTime) @ "ms<br><just:left>\c3Lv\c6" SPC getFormattedNumber(%this.level) SPC "<font:Arial Bold:14>\c2($" @ getFormattedNumber(%this.getUpgradeCost("pick")) @ ")<just:right>\c5x" @ Math_Multiply($Mining::GeneralFluctuation,$Mining::ChanceBuff,3) @ "\c7 :: \c6Health: \c0" @ getFormattedNumber(%this.player.health) SPC "/" SPC getFormattedNumber(%this.maxHealth),-1,1);

	if(200000*(%this.rank+1) <= %this.points) {
		%this.rank++;
		messageAll('',"\c4INFO: \c6" @ %this.name SPC "has achieved rank" SPC %this.rank @ "!");
	}
}

function GameConnection::getUpgradeCost(%this,%item) {
	switch$(%item) {
		case "pick":
			return 250+%this.level*mCeil(%this.level/1.75);
		case "click":
			return 100+((%this.autoClickDelay-400)*-1)*mCeil(((%this.autoClickDelay-400)*-1)/9);
		case "health":
			return 200+(%this.maxHealth-95)*(mFloor((%this.maxHealth-95)/2));
	}
}
function Player::autoClick(%this) {
	if(%this.autoClick)
		cancel(%this.autoClick);
	%this.autoClick = %this.schedule(%this.client.autoClickDelay,autoClick);
	%this.mineBlock();
}

function MinigameSO::economyLoop(%this) {
	if(%this.economyLoop)
		cancel(%this.economyLoop);
	%this.economyLoop = %this.schedule(120000,economyLoop);

	%rand = getRandom(-5,5);
	if(!$Mining::GeneralFluctuation)
		$Mining::GeneralFluctuation = 1;
	$Mining::GeneralFluctuation += %rand/1000;
	messageAll('',"\c2ECONOMY: \c6General value buffs are now at x" @ $Mining::GeneralFluctuation);
}

function MinigameSO::doChanceBlock(%this,%miner) {
	%outcome = getRandom(0,2);
	switch(%outcome) {
		case 0:
			%length = 60000*getRandom(1,3);
			$Mining::ChanceBuff = 1+(getRandom(1,50)/10);
			messageAll('',"\c2CHANCE BLOCK:\c6" SPC %miner.name SPC "mined a chance block! All ores are now worth x" @ $Mining::ChanceBuff SPC "for the next" SPC %length/60000 SPC "minute(s)!");

			%this.schedule(%length,cancelBuff);
			$Mining::ChanceBlockInEffect = 1;
		case 1:
			%length = 60000*getRandom(1,3);
			$Mining::ChanceBuff = 1-(getRandom(10,75)/100);
			messageAll('',"\c2CHANCE BLOCK:\c6" SPC %miner.name SPC "mined a chance block! All ores are now worth x" @ $Mining::ChanceBuff SPC "for the next" SPC %length/60000 SPC "minute(s)!");

			%this.schedule(%length,cancelBuff);
			$Mining::ChanceBlockInEffect = 1;
		case 2:
			%amount = Math_Multiply(getRandom(%miner.level,Math_Multiply(%miner.level,5)),3);
			%miner.cash = Math_Add(%amount,%miner.cash);
			messageClient(%miner,'',"\c2CHANCE BLOCK:\c6 You earned <color:00aa00>$" @ %amount SPC "\c6from the chance block!");
	}
}

function MinigameSO::demandBuffLoop(%this) {
	if(%this.demandBuffLoop)
		cancel(%this.demandBuffLoop);
	%player_count = ClientGroup.getCount();
	%this.demandBuffLoop = %this.schedule(mCeil(3000/%player_count),demandBuffLoop);

	for(%i=0;%i<Mining_OreDatabase.getCount();%i++) {
		%row = Mining_OreDatabase.getObject(%i);
		if(%row.demandBuff < 1.35)
			%row.demandBuff += 0.001;
	}
}

function MinigameSO::cancelBuff(%this) {
	$Mining::ChanceBlockInEffect = 0;
	$Mining::ChanceBuff = 1;
	messageAll('',"\c4INFO: \c6The previous ore value buff has worn off. Chance blocks are now mineable again.");
}

function MinigameSO::checkBrickcount(%this) {
	if(%this.checkBrickcount)
		cancel(%this.checkBrickcount);
	%this.checkBrickcount = %this.schedule(120000,checkBrickcount);

	if(BrickGroup_888888.getCount() > 350000) {
		messageAll('',"\c0WARNING: \c6Brickcount is over 350,000! Clearing the area and resetting...");
		cancel(%this.checkBrickcount);
		BrickGroup_888888.chainDeleteAll();
		%this.initiateBrickcountChecks();
	}
	else
		messageAll('',"\c4INFO: \c6Brickcount is" SPC getFormattedNumber(BrickGroup_888888.getCount(),0));
}
function MinigameSO::initiateBrickcountChecks(%this) {
	if(%this.initiateBrickcountChecks)
		cancel(%this.initiateBrickcountChecks);
	%this.initiateBrickcountChecks = %this.schedule(1000,initiateBrickcountChecks);

	if(BrickGroup_888888.getCount() > 0)
		messageAll('',getFormattedNumber(BrickGroup_888888.getCount(),0) SPC "bricks remaining...");
	if(BrickGroup_888888.getCount() <= 0) {
		cancel(%this.initiateBrickcountChecks);
		Mining_newSpawn();
	}
}