datablock AudioProfile(amb1)
{
	filename = "./sound/ambiance/amb1.wav";
	description = AudioClosest3d;
	preload = true;
};
datablock AudioProfile(amb2:amb1) { filename = "./sound/ambiance/amb2.wav"; };
datablock AudioProfile(amb3:amb1) { filename = "./sound/ambiance/amb3.wav"; };
datablock AudioProfile(amb4:amb1) { filename = "./sound/ambiance/amb4.wav"; };
datablock AudioProfile(amb5:amb1) { filename = "./sound/ambiance/amb5.wav"; };
datablock AudioProfile(amb6:amb1) { filename = "./sound/ambiance/amb6.wav"; };
datablock AudioProfile(amb7:amb1) { filename = "./sound/ambiance/amb7.wav"; };
datablock AudioProfile(amb8:amb1) { filename = "./sound/ambiance/amb8.wav"; };
datablock AudioProfile(amb9:amb1) { filename = "./sound/ambiance/amb9.wav"; };

datablock AudioProfile(drop1)
{
	filename = "./sound/ambiance/drop1.wav";
	description = AudioClosest3d;
	preload = true;
};
datablock AudioProfile(drop2:drop1) { filename = "./sound/ambiance/drop2.wav"; };
datablock AudioProfile(drop3:drop1) { filename = "./sound/ambiance/drop3.wav"; };
datablock AudioProfile(drop4:drop1) { filename = "./sound/ambiance/drop4.wav"; };
datablock AudioProfile(drop5:drop1) { filename = "./sound/ambiance/drop5.wav"; };
datablock AudioProfile(drop6:drop1) { filename = "./sound/ambiance/drop6.wav"; };
datablock AudioProfile(drop7:drop1) { filename = "./sound/ambiance/drop7.wav"; };
datablock AudioProfile(drop8:drop1) { filename = "./sound/ambiance/drop8.wav"; };
datablock AudioProfile(drop9:drop1) { filename = "./sound/ambiance/drop9.wav"; };
datablock AudioProfile(drop10:drop1) { filename = "./sound/ambiance/drop10.wav"; };

function MinigameSO::ambianceLoop(%this) {
	if(%this.ambianceLoop)
		cancel(%this.ambianceLoop);
	%this.ambianceLoop = %this.schedule(getRandom(15000,45000),ambianceLoop);

	%rand = getRandom(1,9);
	serverPlay2D("amb" @ %rand);
	if(%rand == 2)
		Mining_initiateBSide();
}

function MinigameSO::dropLoop(%this) {
	if(%this.dropLoop)
		cancel(%this.dropLoop);
	%this.dropLoop = %this.schedule(getRandom(3500,10000),dropLoop);

	serverPlay2D("drop" @ getRandom(1,10));
}