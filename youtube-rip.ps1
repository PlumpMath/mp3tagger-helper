#Customize this for each computer
$DEFAULT_MUSIC_FOLDER = $env:USERPROFILE + "\Music\_Encoded by me"
$mp3tagger = "E:\SourceControl\Shims\mp3Enhance.exe"

$direxists = [System.IO.Directory]::Exists($env:USERPROFILE + "\Desktop\MusicTemp")
if (!$direxists) 
{
    echo "Temporary directory not found"
    $dirinfo = [System.IO.Directory]::CreateDirectory($env:USERPROFILE + "\Desktop\MusicTemp")
    echo "Temporary directory created"
}
echo "Temporary directory found"
cd $dirinfo.FullName
$url = Read-Host "Youtube Url"
youtube-dl -F $url
$mode = Read-Host "Audio Mode"
youtube-dl -f $mode --audio-format mp3 --audio-quality 0 $url
$m4afile = Get-ChildItem -File -Name *.m4a
$outbitrate = Read-Host "Output bitrate"
ffmpeg -loglevel panic -i $m4afile -acodec libmp3lame -ab $outbitrate $m4afile.Replace(".m4a",".mp3")
echo "Starting MP3 tagging utility"
Start-Process -FilePath $mp3tagger -WorkingDirectory ($env:USERPROFILE + "\Desktop\MusicTemp") -Wait
echo "Deleting M4A"
del *.m4a
$mp3file = Get-ChildItem -File -Name *.mp3
echo ("Moving MP3 to " + $DEFAULT_MUSIC_FOLDER)
Move-Item -Path $mp3file -Destination $DEFAULT_MUSIC_FOLDER
echo "Cleaning up"
[System.IO.Directory]::Delete($env:USERPROFILE + "\Desktop\MusicTemp")
