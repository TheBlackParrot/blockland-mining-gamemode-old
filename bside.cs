function Mining_initiateBSide() {
	for(%i=0;%i<$DefaultMinigame.numMembers;%i++) {
		%client = $DefaultMinigame.member[%i];
		if(isObject(%client.player.light)) {
			%client.hadlight = 1;
			serverCmdLight(%client);
		}
	}

	$Mining::BSideEnabled = 1;
	if($DefaultMinigame.BSideSched)
		cancel($DefaultMinigame.BSideSched);
	$DefaultMinigame.BSideSched = schedule(14500,0,Mining_initiateASide);

	serverCmdEnvGui_setVar(MiningAI,"DirectLightColor","0 0 0");
	serverCmdEnvGui_setVar(MiningAI,"AmbientLightColor","0 0 0");
	serverCmdEnvGui_setVar(MiningAI,"ShadowColor","0 0 0");
	serverCmdEnvGui_setVar(MiningAI,"FogColor","0 0 0");
	serverCmdEnvGui_setVar(MiningAI,"SkyColor","0 0 0");
}

function Mining_initiateASide() {
	serverCmdEnvGui_setVar(MiningAI,"DirectLightColor","0 0 0");
	serverCmdEnvGui_setVar(MiningAI,"AmbientLightColor","0.05 0.05 0.05");
	serverCmdEnvGui_setVar(MiningAI,"ShadowColor","0.07 0.07 0.07");
	serverCmdEnvGui_setVar(MiningAI,"FogColor","0.481155 0.481155 0.528037");
	serverCmdEnvGui_setVar(MiningAI,"SkyColor","0.481155 0.481155 0.528037");
	$Mining::BSideEnabled = 0;

	for(%i=0;%i<$DefaultMinigame.numMembers;%i++) {
		%client = $DefaultMinigame.member[%i];
		if(%client.hadlight) {
			%client.hadlight = 0;
			serverCmdLight(%client);
		}
	}
}

function serverCmdTestBSide(%client) {
	Mining_initiateBSide();
}