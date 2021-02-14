#!/usr/bin/env bash

echo "Variables:"

# Updating manifest
sed -i '' "s|APP_CENTER_IOS|$AC_IOS|g" "$BUILD_REPOSITORY_LOCALPATH"/Collectio/App.xaml.cs
sed -i '' "s|APP_CENTER_DROID|$AC_DROID|g" "$BUILD_REPOSITORY_LOCALPATH"/Collectio/App.xaml.cs
sed -i '' "s|APP_AUTH_HEADER|$AUTH_HEADER|g" "$BUILD_REPOSITORY_LOCALPATH"/Collectio/Repositories/DataRepository.cs
sed -i '' "s|HOST_URL|$HOST_URL|g" "$BUILD_REPOSITORY_LOCALPATH"/Collectio/Utils/RestServiceUtils.cs

# Show Manifest
cat "$BUILD_REPOSITORY_LOCALPATH"/Collectio/App.xaml.cs
cat "$BUILD_REPOSITORY_LOCALPATH"/Collectio/Repositories/DataRepository.cs
cat "$BUILD_REPOSITORY_LOCALPATH"/Collectio/Utils/RestServiceUtils.cs
echo "Manifest updated!"