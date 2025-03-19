# AWS-SecretManagerService-Extensions

Custom extension Library project for AWS Secrets Manager with ASP.NET Core 

Refer: https://vinayakabhat.medium.com/integrating-aws-secrets-manager-with-asp-net-core-a-secure-way-to-manage-secrets-cd6f95469dd9

**Integrating AWS Secrets Manager with ASP.NET Core**

Now, let’s look at how you can integrate AWS Secrets Manager with your ASP.NET Core application using a custom extension. We will define a custom configuration provider that pulls in secrets from AWS dynamically and injects them into your app’s configuration pipeline.

**Install Nuget Package**: https://www.nuget.org/packages/AWS-SecretManagerService-Extensions/

NuGet\Install-Package AWS-SecretManagerService-Extensions -Version 1.0.0

**Set Up AWS Secrets Manager**

Before anything, you’ll need to set up AWS Secrets Manager and store your sensitive data. For example, you can create a secret named my-app-secret with the following JSON structure:

{

 "Settings":

 {
 
  "ConnectionStrings": {
  
    "DefaultConnection": "Server=myServer;Database=myDB;User=myUser;Password=myPassword;"
    
  },
  
  "Endpoints": {
  
    "ServiceA": "https://api.servicea.com",
    
    "ServiceB": "https://api.serviceb.com"
    
  },
  
  "App": {
  
    "Environment": "Production",
    
    "Version": "1.0.0"
    
  },
  
  "AWS": {
  
    "S3AccessKey": "EL*********",
    
    "S3SecretKey": "xyz*******************"
    
  },
  
  "EmailConfig": {
  
    "SmtpServer": "smtp.myemail.com",
    
    "Port": "587",
    
    "Username": "admin@myemail.com",
    
    "Password": "email-password"
    
  }
  
 }
 
}


This secret will be securely managed and can be accessed by your ASP.NET Core application.

**Configuration Example**

To allow your application to easily configure AWS Secrets Manager, you’ll want to include this structure in your appsettings.json:

{

  "AmazonSettings": {
  
    "AWSSecretManager": {
    
      "Region": "your-aws-secret-region",
      
      "SecretName": "my-app-secret"
      
    }
    
  }
  
}

With this setup, AWS Secrets Manager will inject the secrets as part of your configuration at runtime.


**Fetching Secrets in Your Application**

Once the custom provider is in place, it’s time to wire it up in your Program.cs or Startup.cs. Here’s how you can do it:

var builder = WebApplication.CreateBuilder(args);

// Retrieve AWS region and secret name from appsettings.json

string region = builder.Configuration["AmazonSettings:AWSSecretManager:Region"];

string secretName = builder.Configuration["AmazonSettings:AWSSecretManager:SecretName"];

// Add AWS Secrets Manager as a configuration source

builder.Configuration.AddAWSSecretsManager(region, secretName);


This code snippet retrieves the AWS region and secret name from the appsettings.json file and integrates the secrets into the application’s configuration.


**Access Secrets in Your Application**

Now that AWS Secrets Manager is part of your configuration, you can access the secrets just like any other configuration setting in ASP.NET Core:

var connectionString = builder.Configuration["ConnectionStrings:DefaultConnection"];

var ak = builder.Configuration["AWS:S3AccessKey"];

This makes accessing secrets secure, easy, and flexible. If your secrets change in AWS, you don’t need to modify your code or redeploy your app — just update the secret in AWS, and your app will retrieve it dynamically.
