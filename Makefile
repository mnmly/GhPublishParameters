build:
	cd ./GhPublishParameters/bin/Release/net47/ && /Applications/RhinoWIP.app/Contents/Resources/bin/yak build

publish:
	/Applications/RhinoWIP.app/Contents/Resources/bin/yak push $(target)

install:
	/Applications/RhinoWIP.app/Contents/Resources/bin/yak install GhPublishParameters
	/Applications/Rhinoceros.app/Contents/Resources/bin/yak install GhPublishParameters
