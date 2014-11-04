function getExplosionDelay(%radius,%i,%preventspam)
{
	//if((10*%radius)+($Mining::ExplosionsInProgress*100) && !$Mining::HasWarned_ExDe)
	$Mining::ExplosionDelay = ((25*%radius)*(%i+1))+($Mining::ExplosionsInProgress*100);
	if(!$Mining::HasWarned_ExDe && !%preventspam)
	{
		messageAll('',"\c6Explosions are updating at" SPC $Mining::ExplosionDelay SPC "ms.");
		$Mining::HasWarned_ExDe = 1;
	}
}

function Mining_doExplosionStep(%pos,%radius,%o_realtime) {
	InitContainerRadiusSearch(%pos,%radius,$TypeMasks::FXBrickObjectType);
	while((%targetObject = containerSearchNext()) != 0) {
		if(!%targetObject.permanent) {
			%pos_t = %targetObject.getPosition();
			Mining_placeBrick(getWord(%pos_t,0)+4,getWord(%pos_t,1),getWord(%pos_t,2),0,%hit.row_db);
			Mining_placeBrick(getWord(%pos_t,0)-4,getWord(%pos_t,1),getWord(%pos_t,2),0,%hit.row_db);
			Mining_placeBrick(getWord(%pos_t,0),getWord(%pos_t,1)+4,getWord(%pos_t,2),0,%hit.row_db);
			Mining_placeBrick(getWord(%pos_t,0),getWord(%pos_t,1)-4,getWord(%pos_t,2),0,%hit.row_db);
			Mining_placeBrick(getWord(%pos_t,0),getWord(%pos_t,1),getWord(%pos_t,2)+4,0,%hit.row_db);
			Mining_placeBrick(getWord(%pos_t,0),getWord(%pos_t,1),getWord(%pos_t,2)-4,0,%hit.row_db);
			if(%targetObject)
				%targetObject.delete();
		}
	}
	if(%radius != 0)
		schedule((getRealTime() - %o_realtime),0,Mining_doExplosionStep,%pos,%radius-1,getRealTime());
}

function Mining_doExplosion(%pos,%radius) {
	Mining_doExplosionStep(%pos,%radius+5,getRealTime());
}

//function serverCmdExplosionTest(%client,%radius) {
//	%this = %client.player;
//	%eye = vectorScale(%this.getEyeVector(), 8);
//	%pos = %this.getEyePoint();
//	%mask = $TypeMasks::FxBrickObjectType;
//	%hit = firstWord(containerRaycast(%pos, vectorAdd(%pos, %eye), %mask, %this));
//
//	if(!isObject(%hit))
//		return;
//	if(%hit.getClassName() $= "fxDTSBrick" && !%hit.permanent) {
//		talk(%hit.getPosition());
//		Mining_doExplosion(%hit.getPosition(),%radius);
//	}
//}