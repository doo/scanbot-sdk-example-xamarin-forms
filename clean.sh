#!/bin/bash

# Removes bin and obj folders from every project
for folder in `ls -d */`; do
    rm -rf $folder/bin;
    rm -rf $folder/obj;
done

# Removes bin and obj folders from every project in Native.Renderers.Example.Forms
for folder in `ls -d Native.Renderers.Example.Forms/*/`; do
    rm -rf $folder/bin;
    rm -rf $folder/obj;
done
