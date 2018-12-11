## Endpoints

### List properties by postcode

Returns a list of properties matching the given criteria.

```
GET /v1/properties/
```

#### Parameters

- postcode (required)

#### Response

```json
{
"metadata": {
"resultset": {
"count": 5,
"offset": 0,
"limit": 10
},
},
"results": [
{
"propertyReference": "00032896",
"postcode": "AB1 1AA",
"address": "Ross Court 2"
},
{
...etc...
}
]
}
```


### Fetch property by id

Return details of the given property

```
GET /v1/properties/:propertyReference/
```

#### Parameters

- propertyReference: (required) - the Id of the household to retrieve

#### Response

This should mostly mirror the individual results from the List properties by
ID endpoint, with the addition of the `maintainable` boolean. This is `true`
when `no_maint` in UH is `false`, and vice versa.

Properties with `maintainable = false` cannot have repairs raised against them.

```json
{
"propertyReference": "00032896",
"postcode": "AB1 1AA",
"address": "Ross Court 2",
"maintainable": true
}
```

### Create a new repair

Create a repair request, with or without a list of work orders

```
POST /v1/repairs
```

#### Request

```json
{
"propertyReference": "00078345",
"problemDescription": "The fan is buzzing and sometimes not spinning at
all",
"priority": "N",
"contact": {
"name": "Al Smith",
"telephoneNumber": "07876543210",
"emailAddress": "al.smith@hotmail.com",
"callbackTime": "8am - 12pm"
},
"workOrders": [
{
"sorCode":  "20090190"
}
]
}
```

- propertyReference - A Hackney-specific code identifying the property
- problemDescription - A free-text description of the problem which needs to
be fixed
- priority - A single-character representation of the repair priority:
- N - Normal
- U - Urgent
- I - Immediate
- E - Emergency
- G -
- Z -
- V -
- contact - The person who should be contacted in relation to the repair
- callbackTime (optional) - a time which this person has specified that they
are available to be called back. Don't pass this key at all if it is not
applicable
- workOrders (optional) - A list of the repair jobs which need to happen to
fix the resident's problem. Don't pass this key at all if it is not
applicable
- sorCode - a "Schedule of Rates" code which describes the problem

#### Response

A successful request should return HTTP 201 Created

The response contains the data which was submitted, plus some additional IDs
and data calculated based on the SOR code.

```json
{
"repairRequestReference": "08912445",
"propertyReference": "00078345",
"problemDescription": "The fan is buzzing and sometimes not spinning at
all",
"priority": "N",
"contact": {
"name": "Al Smith",
"telephoneNumber": "07876543210",
"emailAddress": "al.smith@hotmail.com",
"callbackTime": "8am - 12pm"
},
"workOrders": [
{
"workOrderReference": "20090190",
"sorCode":  "20090190",
"supplierReference": "00000127"
}
]
}
```

- repairRequestReference - an identifier for the repair request made by the resident
- workOrderReference - an identifier for the work to be done by a contractor
- supplierReference - an identifier for the organisation who will carry out
the work - e.g. the Hackney DLO

If the repair was created without work orders, an empty array of workOrders
should be returned (rather than no key or a null value)

### Retrieve a repair by reference

Retrieve a repair request, with or without a list of work orders

```
GET /v1/repairs/:repairRequestReference
```

#### Parameters

- Repair request reference (required)

#### Response

The response contains the same data as when the repair was created:

```json
{
"repairRequestReference": "08912445",
"propertyReference": "00078345",
"problemDescription": "The fan is buzzing and sometimes not spinning at
all",
"priority": "N",
"contact": {
"name": "Al Smith",
"telephoneNumber": "07876543210",
"emailAddress": "al.smith@hotmail.com",
"callbackTime": "8am - 12pm"
},
"workOrders": [
{
"workOrderReference": "20090190",
"sorCode":  "20090190",
"supplierReference": "00000127"
}
]
}
```

As above: if the repair was created without work orders, an empty array of
workOrders should be returned (rather than no key or a null value)

### Get appointment booked for a Work Order

Returns the appointment booked for a work order

```
GET /v1/work_orders/:workOrderReference/appointments/
```

#### Parameters

- Work order reference (required)

#### Response

```json
{
"beginDate": "2017-10-18T08:00:00Z",
"endDate": "2017-10-18T12:00:00Z",
}
```

### Book an appointment for a Work Order

Creates the appointment in DRS and returns the booked appointment

```
POST /v1/work_orders/:workOrderReference/appointments/
```

#### Parameters

###### Specified in path

- workOrderReference - Work order reference (required)

###### Specified in JSON body

- beginDate - The start time for the desired appointment (required)
- endDate - The end time for the desired appointment (required)

#### Response

A successful response should book the appointment in DRS

```json
{
"beginDate": "2017-10-18T08:00:00Z",
"endDate": "2017-10-18T12:00:00Z",
}
```

### Get available appointments for Work Order

Returns a list of available appointments for a work order

```
GET /v1/work_orders/:workOrderReference/available_appointments/
```

#### Parameters

- Work order reference (required)

#### Response
A successful response should create the Work order in DRS and get the list of available appointments.

```json
{
"metadata": {
"resultset": {
"count": 5,
"offset": 0,
"limit": 10
},
},
"results": [
{
"beginDate": "2017-10-18T08:00:00Z",
"endDate": "2017-10-18T12:00:00Z",
"bestSlot": true
},
{
"beginDate": "2017-10-18T12:00:00Z",
"endDate": "2017-10-18T16:15:00Z",
"bestSlot": false
},
{
"beginDate": "2017-10-19T10:00:00Z",
"endDate": "2017-10-19T14:30:00Z",
"bestSlot": false
},
{
"beginDate": "2017-10-20T08:00:00Z",
"endDate": "2017-10-20T16:15:00Z",
"bestSlot": false
},
{
"beginDate": "2017-10-20T16:00:00Z",
"endDate": "2017-10-20T18:00:00Z",
"bestSlot": false
},
{
...etc...
}
]
}
```
### Get account and address for housing residents

Returns a list of account and address information

```
Get /v1/accounts/verifyhousingaccountlogindetail?parisReference=123434470&postcode=E8 1HH
```

#### Parameters
- Paris reference (required)
- Postcode (required)

#### Response
A successful response should get a list of account and address information corresponding to the required parameters.

```json
{
"results": [
{
"parisReferenceNumber": "123403470",
"postcode": "E8 1HH",
"address": "Maurice Bishop House"
},
{
...etc...
}
]
}
```

### Get housing transactions for residents

Returns a list of transactions

```
Get /v1/ transactions?tagReference=123456/01
```

#### Parameters
- Tag Reference (required)

#### Response
A successful response should get a list of transactions based on the tag reference provided.

```json
{
"results": [
{
"tagReference": "123456/01",
"propertyReference": "01234513",
"transactionSid": null,
"houseReference": "000123",
"transactionType": "RPO",
"postDate": "2017-11-28T00:00:00",
"realValue": -10,
"transactionID": "49ct627-e9d4-e711-8109-zzz71b7fe041",
"debDesc": "PayPoint/Post Office"
},
{
"tagReference": "123456/01",
"propertyReference": "01234513",
"transactionSid": null,
"houseReference": "123456",
"transactionType": "RTB",
"postDate": "2017-11-27T00:00:00",
"realValue": -110.95,
"transactionID": "c4396c29-87o3-e711-8109-zzz71b7fe041",
"debDesc": "Housing Benefit"
},
{
...etc...
}
]
}
```

### Get payment agreement information for housing residents

Returns a list of payment agreement information

```
Get /v1/accountpaymentagreement?TagRef=12345/01
```

#### Parameters
- Tag reference (required)

#### Response
A successful response should get a list of payment agreement information corresponding to the given tag reference.
```json
{
"results": [
{
"agreementAmount": "47.18",
"agreementFrequency": "1",
"agreementId": "471po9ia-dcd4-e711-8109-e00zzz7fe0bn"
},
{
...etc...
}
]
}
...

### Get account information for housing residents

```
Get  /v1/accounts/accountdetailsbyparisreference?parisReference=123409789
```

#### Parameters
- Parisreferencenumber (required)

#### Response
A successful response should get a list of account information corresponding to the given Parisreferencenumber.
```json
{
"results": [
{
"propertyReferenceNumber": "1234528",
"benefit": "30",
"tagReferenceNumber": "123456/01",
"accountid": "93d621ae-hgc7-e711-8111-70106faa6a11",
"currentBalance": "45.35",
"rent": "9.04",
"housingReferenceNumber": "145656",
"directdebit": null,
"personNumber": null,
"responsible": false,
"title": "Mr",
"forename": "Andy",
"surname": "Benj"
},
{
...etc...
}
]
}

```
### Authenticate users based on Username and Password

Returns the user detail
Get/v1/login/authenticatenhoofficers?username=uaccount&password=hackney
```
#### Parameters
- Username (required)
- Password (required)

#### Response
A successful response should get the detail of Authenticated neighbourhood officers corresponding to the required parameters.

```json
{
"result": {
"userId": "de98e4b6-15dc-e711-8115-701brfaabb11",
"firstName": "Shweta",
"surName": "Sandilya",
"activeDirectoryUserName": "ssandilya"
}
}

```
