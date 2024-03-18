# �������
Param(
    # NuGet APIKey
    [string] $apikey,
   # NuGet Դ��Ĭ��ʹ�� nuget.org ��Դ
    [string] $source = "https://api.nuget.org/v3/index.json"

)
Write-Host "apikey��: $apikey"
Write-Host "nuget.orgԴ��: $source"
Write-Warning "���ڷ��� framework Ŀ¼ NuGet ��......";

# ���� .\framework\nupkgs ������Ŀ¼
cd .\framework\nupkgs;
$framework_nupkgs = Get-ChildItem -Filter *.nupkg;

# �������� *.nupkg �ļ�
for ($i = 0; $i -le $framework_nupkgs.Length - 1; $i++){
    $item = $framework_nupkgs[$i];

    $nupkg = $item.FullName;
    $snupkg = $nupkg.Replace(".nupkg", ".snupkg");

    Write-Output "-----------------";
    $nupkg;

    # ������ nuget.org ƽ̨
    dotnet nuget push $nupkg --skip-duplicate --api-key $apikey --source  $source;
    dotnet nuget push $snupkg --skip-duplicate --api-key $apikey --source  $source;

    Write-Output "-----------------";
}

# �ص���Ŀ��Ŀ¼
cd ../../;

Write-Warning "�����ɹ�";