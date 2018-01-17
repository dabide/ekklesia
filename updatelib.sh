#!/bin/bash
errorhandler() {
    # Modified verion of http://stackoverflow.com/a/4384381/352573
    errorcode=$?
    echo "Error $errorcode"
    echo "The command executing at the time of the error was"
    echo "$BASH_COMMAND"
    echo "on line ${BASH_LINENO[0]}"
    exit $errorcode
}

DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" && pwd )"
LIB=${DIR}/lib
PATCHVERSION=$(($( date +%s ) / 86400))
TAG=v5.0.2

# From now on, catch errors
trap errorhandler ERR

echo "Cleaning up"
rm -rf "${LIB}/ServiceStack*"
mkdir -p "${LIB}/ServiceStack"

if [ -d "${LIB}/src/ServiceStack" ]; then
    echo "Deleting ServiceStack repo"
    rm -rf "${LIB}/src/ServiceStack"
fi

if [ -d "${LIB}/src/ServiceStack.OrmLite" ]; then
    echo "Deleting ServiceStack.OrmLite repo"
    rm -rf "${LIB}/src/ServiceStack.OrmLite"
fi

if [ -d "${LIB}/src/ServiceStack.Redis" ]; then
    echo "Deleting ServiceStack.Redis repo"
    rm -rf "${LIB}/src/ServiceStack.Redis"
fi

echo "Cloning ServiceStack repo"
git clone --depth 1 --branch ${TAG} https://github.com/ServiceStack/ServiceStack "${LIB}/src/ServiceStack"

echo "Cloning ServiceStack.OrmLite repo"
git clone --depth 1 --branch ${TAG} https://github.com/ServiceStack/ServiceStack.OrmLite "${LIB}/src/ServiceStack.OrmLite"

echo "Cloning ServiceStack.Redis repo"
git clone --depth 1 --branch ${TAG} https://github.com/ServiceStack/ServiceStack.Redis "${LIB}/src/ServiceStack.Redis"

DEST=${LIB}/tmp
dotnet publish "${LIB}/src/ServiceStack.OrmLite/src/ServiceStack.OrmLite.Sqlite/ServiceStack.OrmLite.Sqlite.csproj" -c Release -f netstandard2.0 -o "${DEST}"
dotnet publish "${LIB}/src/ServiceStack.OrmLite/src/ServiceStack.OrmLite.SqlServer/ServiceStack.OrmLite.SqlServer.csproj" -c Release -f netstandard2.0 -o "${DEST}"
dotnet publish "${LIB}/src/ServiceStack.OrmLite/src/ServiceStack.OrmLite.PostgreSQL/ServiceStack.OrmLite.PostgreSQL.csproj" -c Release -f netstandard2.0 -o "${DEST}"
dotnet publish "${LIB}/src/ServiceStack.OrmLite/src/ServiceStack.OrmLite.MySql/ServiceStack.OrmLite.MySql.csproj" -c Release -f netstandard2.0 -o "${DEST}"
dotnet publish "${LIB}/src/ServiceStack.Redis/src/ServiceStack.Redis/ServiceStack.Redis.csproj" -c Release -f netstandard2.0 -o "${DEST}"
dotnet publish "${LIB}/src/ServiceStack/src/ServiceStack/ServiceStack.csproj" -c Release -f netstandard2.0 -o "${DEST}"
dotnet publish "${LIB}/src/ServiceStack/src/ServiceStack.Server/ServiceStack.Server.csproj" -c Release -f netstandard2.0 -o "${DEST}"
dotnet publish "${LIB}/src/ServiceStack/src/ServiceStack.Mvc/ServiceStack.Mvc.csproj" -c Release -f netstandard2.0 -o "${DEST}"

mv "${DEST}/ServiceStack"* "${LIB}/ServiceStack"

rm -rf "${DEST}"
rm -rf "${LIB}/src/*"

echo "Finished succesfully"