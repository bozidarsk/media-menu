# build: mono, gtk-sharp-3
# runtime: mono, gtk-sharp-3, gtk-layer-shell, playerctl, libinput, glib-2.0

mcs '-recurse:*.cs' -pkg:gtk-sharp-3.0 -out:media-menu
if [[ $? -eq 0 ]]; then
	chmod 755 ./media-menu
	sudo chown root:root ./media-menu
	sudo mv ./media-menu /usr/local/bin
else
	echo "Build failed."
	exit 1
fi
