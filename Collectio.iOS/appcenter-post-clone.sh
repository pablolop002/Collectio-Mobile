#!/usr/bin/env bash

echo "Variables:"

# Updating manifest
sed -i '' "s|APPCENTER_IOS|$APP_IOS|g" "$BUILD_REPOSITORY_LOCALPATH"/Collectio/App.xaml.cs
sed -i '' "s|APPCENTER_DROID|$APP_DROID|g" "$BUILD_REPOSITORY_LOCALPATH"/Collectio/App.xaml.cs
sed -i '' "s|HOSTURL|$HOSTURL|g" "$BUILD_REPOSITORY_LOCALPATH"/Collectio/Repositories/DataRepository.cs

# Show Manifest
cat "$BUILD_REPOSITORY_LOCALPATH"/Collectio/App.xaml.cs
echo "Manifest updated!"