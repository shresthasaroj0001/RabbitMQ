# RabbitMQ Producer-Consumer System with Dead-Letter Handling and QoS

This project implements a robust producer-consumer system using RabbitMQ, incorporating dead-letter handling, Quality of Service (QoS), and support for long-running jobs. It provides a scalable solution for processing items asynchronously while ensuring reliability and flexibility.

## Table of Contents

- [Overview](#overview)
- [Features](#features)
- [Prerequisites](#prerequisites)
- [Installation](#installation)
- [Configuration](#configuration)
- [Usage](#usage)
- [Endpoints](#endpoints)

## Overview

This system consists of three main components:
1. Producer (ASP.NET Core Web API): Sends itemId to RabbitMQ queue and stores item status in database.
2. Consumer (ASP.NET Core Web API): Processes items from RabbitMQ queue, handles retries, and manages long-running jobs.
3. RabbitMQ: Manages message queues, dead-letter exchange, and Quality of Service settings.

The system supports the following features:
- Asynchronous processing of items
- Dead-letter handling for failed messages
- Quality of Service (QoS) to ensure one item processed at a time
- Support for long-running jobs
- Ability to pause and resume consuming
- Scheduled consuming periods

## Features

- RESTful API for adding and removing items
- Background task management for consuming messages
- Dead-letter handling with retry mechanism
- QoS support for reliable processing
- Long-running job support
- Pause/resume functionality for consuming
- Scheduled consuming periods
- Status monitoring for consuming activity

## Prerequisites

- .NET Core SDK 6.0+
- RabbitMQ server installed and running

## Installation

1. Clone this repository:

2. Navigate to the project directory:

3. Restore NuGet packages:

4. Build the solution:

5. Run the application:


## Configuration

Adjust the configuration according to your RabbitMQ setup and requirements in the program.cs of producer and consumer project

## Usage

To use the application, you can send HTTP requests to the following endpoints:

### Producer Endpoints

1. Add Item:
2. Remove Item:


### Consumer Endpoints

1. Pause Consuming:

2. Resume Consuming:

3. Set Scheduled Consuming Period:

4. Get Consumption Status:
