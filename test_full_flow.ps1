[System.Net.ServicePointManager]::ServerCertificateValidationCallback = { $true }
$baseUrl = "http://localhost:5219"

function Test-GraphQL {
    param($token, $query, $variables)
    $headers = @{}
    if ($token) { $headers["Authorization"] = "Bearer $token" }
    $body = @{ query = $query }
    if ($variables) { $body["variables"] = $variables }
    try {
        $response = Invoke-RestMethod -Uri "$baseUrl/graphql" -Method Post -Body ($body | ConvertTo-Json -Depth 5) -ContentType 'application/json' -Headers $headers
        return $response
    }
    catch {
        Write-Host "GraphQL Error: $_"
        if ($_.Exception.Response) {
            $reader = New-Object System.IO.StreamReader($_.Exception.Response.GetResponseStream())
            Write-Host $reader.ReadToEnd()
        }
        return $null
    }
}

# 1. Login
Write-Host "`n--- 1. Logging in ---"
try {
    $loginBody = @{ email = "admin@machly.com"; password = "Admin123!" }
    $loginResponse = Invoke-RestMethod -Uri "$baseUrl/auth/login" -Method Post -Body ($loginBody | ConvertTo-Json) -ContentType 'application/json'
    $token = $loginResponse.token
    Write-Host "Token obtained: $($token.Substring(0,10))..."
}
catch {
    Write-Host "Login Failed: $_"
    exit
}

# 2. Query Machines
Write-Host "`n--- 2. Query Machines ---"
$machinesQuery = "{ machines { id title pricePerDay tariffsAgro { hectarea } } }"
$machines = Test-GraphQL -token $token -query $machinesQuery
if ($machines.data.machines) {
    Write-Host "Machines found: $($machines.data.machines.Count)"
    $firstMachine = $machines.data.machines[0]
    Write-Host "First Machine: $($firstMachine.title) - ID: $($firstMachine.id)"
    $machineId = $firstMachine.id
}
else {
    Write-Host "No machines found or error."
    $machines | ConvertTo-Json -Depth 5
    exit
}

# 3. Query Me
Write-Host "`n--- 3. Query Me ---"
$meQuery = "{ me { id name email role } }"
$me = Test-GraphQL -token $token -query $meQuery
Write-Host "Me: $($me.data.me.name) ($($me.data.me.email)) Role: $($me.data.me.role)"

# 4. Query Machine by ID
Write-Host "`n--- 4. Query Machine by ID ---"
$machineByIdQuery = "query GetMachine(`$id: String!) { machineById(id: `$id) { id title description pricePerDay } }"
$machineByIdVars = @{ id = $machineId }
$machineById = Test-GraphQL -token $token -query $machineByIdQuery -variables $machineByIdVars
if ($machineById.data.machineById) {
    Write-Host "MachineById: $($machineById.data.machineById.title) - $($machineById.data.machineById.id)"
}
else {
    Write-Host "MachineById query failed or returned null"
    $machineById | ConvertTo-Json -Depth 5
}

# 5. Mutation CreateBooking (requires RENTER role, admin may not have it)
Write-Host "`n--- 5. Mutation CreateBooking ---"
$bookingMutation = "mutation CreateBooking(`$input: CreateBookingInput!) { createBooking(input: `$input) { id status totalPrice } }"
$bookingVars = @{ input = @{ machineId = $machineId; start = "2025-01-02T10:00:00Z"; end = "2025-01-05T10:00:00Z" } }
$booking = Test-GraphQL -token $token -query $bookingMutation -variables $bookingVars
if ($booking.errors) {
    Write-Host "Booking Error (expected if Admin): $($booking.errors[0].message)"
}
else {
    Write-Host "Booking Created: $($booking.data.createBooking.id)"
}
