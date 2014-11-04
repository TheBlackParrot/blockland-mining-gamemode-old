function encryptPoemLine(%poem,%line,%way) {
	%line = $Mining::Poem[%poem,%line];

	switch(%way) {
		case 1:
			%charset = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890";
			for(%i=0;%i<strLen(%line);%i++)
				%encrypt = %encrypt @ getSubStr(%charset,getRandom(0,strLen(%charset)),1);
			return %encrypt;

		case 2:
			%charset = %line;
			for(%i=0;%i<strLen(%line);%i++) {
				if(getSubStr(%line,%i,1) $= " ")
					%encrypt = %encrypt @ " ";
				else
					%encrypt = %encrypt @ getSubStr(%charset,getRandom(0,strLen(%charset)),1);
			}
			return %encrypt;
	}
}