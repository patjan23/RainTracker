
# RainTacker 

Created end points for raintracker API

## Features  
- GET : Gets lists all the rain record for a specfic user  
- POST : Write the rain record for a specfic user 

## Testing
API Endpoints
POST http://localhost:8080/api/data
bashcurl -X POST http://localhost:8080/api/data \
  -H "Content-Type: application/json" \
  -H "x-userId: PatJan" \
  -d '{"rain": true}'
GET http://localhost:8080/api/data
bashcurl -X GET http://localhost:8080/api/data \
  -H "x-userId: PatJan"  
  
## Test cases 
dotnet test 
