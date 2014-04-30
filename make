#!/bin/sh

MSBUILD_PATH="C:/Windows/Microsoft.NET/Framework64/v4.0.30319/MSBuild.exe"
NUGET_PATH="tools/NuGet.exe"
NUGET_SOURCE="https://www.myget.org/F/vtexlab/"
NUNIT_PATH="packages/NUnit.Runners.2.6.3/tools/nunit-console.exe"

SOLUTION_FILE="SemanticVersioning.sln"
NUGET_PROJECTS=("SemanticVersioning/SemanticVersioning.csproj")
TEST_ASSEMBLIES=("SemanticVersioning.Tests/SemanticVersioning.Tests.csproj")

# Default
if [ "$1" == "" ] || [ "$1" == "all" ]; then
  echo " --- make: NuGet Restore, Rebuild solution and Run NUnit tests"
  make restore && make rebuild && make test

# Build Solution
elif [ "$1" == "build" ]; then
  echo
  echo " --- make: Build solution"
  $MSBUILD_PATH "$SOLUTION_FILE" /verbosity:m /property:Configuration=Release /target:Build

# Build Solution
elif [ "$1" == "rebuild" ]; then
  echo
  echo " --- make: Rebuild solution"
  $MSBUILD_PATH "$SOLUTION_FILE" /verbosity:m /property:Configuration=Release /target:Clean,Build

# Run Tests
elif [ "$1" == "test" ]; then
  echo
  echo " --- make: Run NUnit tests"
  $NUNIT_PATH "${TEST_ASSEMBLIES[@]}"

# Restore NuGet Packages
elif [ "$1" == "restore" ]; then
  echo
  echo " --- make: NuGet Restore"
  $NUGET_PATH restore "$SOLUTION_FILE"
 
# Pack NuGet Packages
elif [ "$1" == "pack" ]; then
  echo " --- make: Build NuGet package"
  if test ! -e "nuget"; then mkdir "nuget"; fi
  rm nuget/*
  for ((i = 0; i < ${#NUGET_PROJECTS[@]}; i++)); do
    $NUGET_PATH pack "${NUGET_PROJECTS[$i]}" -o nuget -Build -p Configuration=Release -Symbols
    echo
  done
  
# Push NuGet Packages
elif [ "$1" == "push" ]; then
  echo " --- make: Push NuGet package"
  for f in `ls nuget`; do
    $NUGET_PATH push "nuget/$f"
    echo
  done

# Command not found
else
    echo " :: Command '$1' not found"
fi
