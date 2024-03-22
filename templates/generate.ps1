# ��ȡ���� MoYu + EFCore ģ��Ŀ¼
$efcore_path = pwd;
$efcore_templates = Get-ChildItem -Directory $efcore_path -Exclude SqlSugarTemplates;

# ��ȡ���� MoYu + SqlSugar ģ��Ŀ¼
cd .\SqlSugarTemplates;
$sqlsugar_path = pwd;
$sqlsugar_templates = Get-ChildItem -Directory $sqlsugar_path;
cd ..;

# �������� Nupkg ��
function generate($path, $templates){
    $dir = $path.Path;
    Write-Warning "�������� [$dir] Nupkg ��......";

    # ��������ģ��
    for ($i = 0; $i -le $templates.Length - 1; $i++){
        $item = $templates[$i];

        # ��ȡ����·��
        $fullName = $item.FullName;

        Write-Output "-----------------";
        $fullName
        Write-Output "-----------------";

        # ���� .nupkg ��
        .\nuget.exe pack $fullName;
    }

    Write-Output "���ɳɹ�";
}

# ���� EFCore Nupkg ��
generate -path $efcore_path -templates $efcore_templates

# ���� SqlSugar Nupkg ��
generate -path $sqlsugar_path -templates $sqlsugar_templates