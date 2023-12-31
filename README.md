# FileTagServiceGPT

## Introduction

FileTagServiceGPT is a .NET 7 microservice designed to automatically generate and assign tags to PDF documents using the OpenAI GPT model. This service is particularly adept at monitoring file uploads in a Supabase environment, thereby facilitating efficient file management and retrieval through advanced tagging.

## Features

- **OpenAI GPT Integration**: Leverages OpenAI's GPT model to produce contextually relevant tags for PDF documents.
- **Supabase Compatibility**: Automatically monitors and processes PDF files uploaded to a Supabase database.
- **.NET 7 Framework**: Built on the robust .NET 7 platform, ensuring high performance and compatibility with modern infrastructure.

## Installation

1. Clone the repository:
2. Navigate to the cloned directory:
3. Install the required dependencies (specific commands may vary depending on the setup).

## Configuration

### Environment Variables

The service requires the following environment variables to be set:

- `SUPABASE_URL`: The URL of your Supabase instance.
- `SUPABASE_KEY`: Your unique Supabase key.
- `OpenAI_KEY`: Your OpenAI API key for accessing GPT models.

### Supabase Setup

- Ensure that all necessary models are correctly defined within your Supabase database.

## Usage

After setting up the environment variables and ensuring your Supabase database is configured:

1. Compile and run the .NET 7 project.
2. The service will automatically start monitoring Supabase for new PDF file uploads and process them accordingly.

*Further usage instructions can be provided depending on the project's specifics.*

## Contributing

We welcome contributions to enhance FileTagServiceGPT. If you wish to contribute:

1. Fork the repository.
2. Create a new branch (`git checkout -b feature/YourFeature`).
3. Commit your changes (`git commit -am 'Add some feature'`).
4. Push to the branch (`git push origin feature/YourFeature`).
5. Create a new Pull Request.

Ensure your contributions follow the coding standards of the project and include necessary tests.