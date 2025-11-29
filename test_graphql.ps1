[System.Net.ServicePointManager]::ServerCertificateValidationCallback = {$true}
try {
    Write-Host "Testing GraphQL..."
    $response = Invoke-RestMethod -Uri 'http://localhost:5219/graphql' -Method Post -Body '{"query": "{ machines { id title } }"}' -ContentType 'application/json'
    $response | ConvertTo-Json -Depth 5
} catch {
    Write-Host "Error: $_"
    if ($_.Exception.Response) {
        $stream = $_.Exception.Response.GetResponseStream()
        $reader = New-Object System.IO.StreamReader($stream)
        $reader.ReadToEnd()
    }
}
