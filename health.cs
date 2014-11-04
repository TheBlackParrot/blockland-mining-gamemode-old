function Player::modifyHealth(%this,%amount) {
	%this.health += %amount;
	if(%this.health >= %this.client.maxHealth)
		%this.health = %this.client.maxHealth;
	if(%amount < 0)
		%this.setDamageFlash((%amount/10)*-1);
	if(%this.health <= 0) {
		%this.health = 0;
		%this.kill();
	}
}

function MinigameSO::regeneratePlayers(%this) {
	if(%this.regeneratePlayers)
		cancel(%this.regeneratePlayers);
	%this.regeneratePlayers = %this.schedule(5000,regeneratePlayers);

	for(%i=0;%i<%this.numMembers;%i++) {
		if(%this.member[%i].player) {
			%this.member[%i].player.modifyHealth(1);
		}
	}
}