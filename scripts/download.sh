#!/usr/bin/env bash

read -p "url: " url
read -p "token: " token
read -p "backup name: " name

if [[ $url != http* ]]; then
  url="https://$url"
fi

fetch() {
    local url="$1"
    local token="$2"
    local outpath="$3"

    if [[ -z $outpath ]]; then
        outpath="/dev/stdout"
    fi

    curl -s $url -H "X-Token: $token" -o $outpath
}

dates=$(fetch "$url/backups/$name/generations" $token | jq -r '.[].date')

echo "available generations:"

id=0
for date in $dates; do
    echo "$id: $date"
    id=$((id+1))
done

read -p "generation id: " id

files=$(fetch "$url/backups/$name/generations/$id" $token | jq -r '.[]')

mkdir -p $name
cd $name

for file in $files; do
    echo "downloading $file"
    dir=$(dirname $file)
    mkdir -p $dir
    fetch "$url/backups/$name/generations/$id/$file" $token "$file" &
done
