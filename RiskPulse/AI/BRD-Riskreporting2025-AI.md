# Business Requirement Document (BRD) — Ver 1.0

**Company:** Siyapatha Finance PLC
**Status:** Final
**Final Date:** 29 May 2025

## Document Metadata

- **Project Name:** Develop a risk reporting system.
- **Requested by:** Amal Sandaruwan
- **Requested date:** 29 May 2025
- **Reference Number:** Old No 083635/117984
- **Status:** Final

## Preface

The Business Requirements Document (BRD) is essential in the project planning phase. It lays a strong foundation for success by clearly defining business requirements, reducing the risk of project failure. Beyond that, it fosters stakeholder alignment, enhances collaboration, and promotes transparency. A well-maintained BRD also helps save time and costs by minimizing change requests. All stakeholders must review and approve this document. Along with the Functional Specification Document, the BRD sets the standard for determining the project's successful completion.

## Instructions to Requester

Do not remove any section of this document, as each is designed to capture specific details about the requirement. Enter your requirements directly after the instructions, ensuring clarity and specificity.

Once completed, submit the BRD to the Project Manager for implementation. After review and acceptance, the Functional Specification Document (FSD) will be shared with you. The FSD outlines the proposed solution, recommendations for improvement, constraints, risks, and other relevant considerations.

## Executive Summary

Risk reporting is a critical component of the risk management process. Currently, we collect risk data through email using Excel files, which are then used to prepare risk reports. This manual process is time-consuming, prone to delays, and susceptible to data entry errors. By automating the risk reporting process and implementing a dedicated system, we can significantly enhance the efficiency, accuracy, and timeliness of our risk management activities. This improvement will strengthen our overall risk management process and support better decision-making.

## Stakeholders

The Stakeholders play a key role in ensuring the successful implementation of the project. They must be accountable for specific tasks and decisions throughout the project's execution. It is important to clearly define their roles and responsibilities.

- **Accountable Executive** — Takes overall ownership of the project, ensures alignment with business objectives, and removes obstacles.
- **Business Owner(s)** — Represents the business area directly responsible for or impacted by the change. Defines requirements, validates solutions, and ensures business readiness.
- **Strategic Sponsor** — Sponsors the project, secures cost approvals, and manages funding.
- **Key Stakeholders** — Individuals or departments affected by the change, providing input and ensuring compliance. Example: Operations team, Compliance & Risk Teams.

### Stakeholder List

| Stakeholder Name | Role |
|---|---|
| Indraka Liyanage | CRO / Business Owner |
| Amal Sandaruwan | Accountable Executive |
| Lucian Devotta | Strategic Sponsor |
| All Branches and dept. | Key Stakeholders |

## Business Objectives

The objective of this project is to automate the risk reporting process to ensure the timely and accurate collection of risk data. By implementing a dedicated system, we aim to eliminate the inefficiencies and errors associated with the current manual process. This will enable us to maintain the high-quality data necessary for effective risk management analysis, ultimately strengthening our risk management framework and improving decision-making capabilities.

## Benefit of Change

Automating the risk reporting process will bring several key benefits to the organization:

- **Improved Efficiency** — Eliminating manual data collection and report preparation will reduce time spent on administrative tasks, allowing risk management teams to focus on analysis and decision-making.
- **Enhanced Accuracy** — Automation will minimize human errors associated with manual data entry, ensuring more reliable and consistent risk data.
- **Timely Risk Insights** — A dedicated system will facilitate real-time data collection and reporting, enabling proactive risk management and faster decision-making.
- **Regulatory Compliance** — Improved data accuracy and timely reporting will help the company comply with regulatory requirements and internal risk governance standards.
- **Better Risk Visibility** — Centralized and automated reporting will provide management with clear, up-to-date insights into emerging risks, enhancing overall risk oversight.
- **Resource Optimization** — Reducing reliance on manual processes will optimize staff utilization, allowing resources to be allocated to higher-value risk mitigation activities.

By implementing this change, the organization will strengthen its risk management framework, improve decision-making, and foster a more proactive risk culture.

## Current Solution

The existing risk reporting process is manual and relies on Excel files and email communication. Below is a step-by-step breakdown:

1. **Risk Department Distributes Reporting Format** — The Risk Department prepares an Excel-based risk reporting template and sends it to all branches and departments via email. This template serves as a standardized format for collecting risk-related data.
2. **Branches and Departments Input Data** — Each branch and department is responsible for filling in the template with relevant risk data, such as identified risks, incidents, and control measures.
3. **Submission via Email** — Once completed, the branches and departments attach the updated Excel files to an email and send them back to the Risk Department.
4. **Manual Compilation of Reports** — The Risk Department collects all submitted Excel files, consolidates the data, and manually prepares the final risk reports for management review.

### Challenges of the Current Solution

- **Time-Consuming** — Manually collecting, consolidating, and verifying multiple Excel files takes significant time.
- **Prone to Errors** — Data entry mistakes and inconsistencies may occur due to manual input.
- **Delays in Reporting** — Dependence on email submissions can lead to delays, affecting the timeliness of risk analysis.
- **Lack of Real-Time Data** — Since updates occur periodically, risk data is not available in real-time for proactive decision-making.

By automating this process, these challenges can be mitigated, leading to improved efficiency, accuracy, and timely risk insights.

## Business Objective and Scope

The current manual process for risk reporting involves the distribution and collection of Excel files via email, which is inefficient and prone to errors. This method results in delayed reporting, data entry inaccuracies, and increased administrative workload, all of which can compromise the quality and timeliness of risk management analysis.

To address these challenges, there is a critical need to implement an automated risk reporting system. This system will streamline the data collection process, reduce the potential for errors, and ensure that accurate and timely data is available for analysis. By automating these processes, we can significantly improve the efficiency of our risk management activities, enhance data integrity, and support more informed decision-making.

The scope of this project includes automating the current Excel-based risk reporting processes for various critical reports. Specifically, the automation will cover the following reports:

- Key Risk Indicators (KRI) — Monthly
- Self-Assessment Questionnaire (SAQ) — Monthly
- Loss Event Report — Monthly
- Risk Register — Monthly

The automation process will involve streamlining data collection, report generation, and distribution, ensuring that these reports are produced in a timely, accurate, and consistent manner. This initiative aims to reduce manual effort, minimize errors, and enhance the overall efficiency of the risk management process.

## Non-Functional Requirements

### 1. User Access & Role Management

- **Maker Level Users** — Each branch or department will have **2 Maker profiles** responsible for entering risk data into the system.
- **Checker Level Users** — Each branch or department will have **2 Checker profiles** responsible for verifying and approving the data entered by Makers before submission.
- **Monitoring & Review Access** — The Risk Department will have **8 staff members** assigned with monitoring and review access to oversee all risk submissions.
- **Scalability of User IDs** — As the number of branches and departments increases, the system must dynamically allow for the addition of new users while maintaining role-based access control.

### 2. System Volume & Growth Expectations

- **Current Volume** — The system should initially support risk reporting for the existing number of branches and departments with structured Maker, Checker, and Reviewer access.
- **Expected Growth** — The number of branches and departments is expected to increase over time, requiring a scalable system that can accommodate additional users without performance degradation.

### 3. System Performance & Availability

- **System Uptime** — The solution should maintain at least **99.9% uptime**, ensuring continuous availability for risk data submission and reporting.
- **Response Time** — The system should process and display risk data within **3 seconds** of submission to ensure a seamless user experience.

### 4. Security & Access Control

- **Role-Based Access** — The system must enforce strict role-based access, ensuring that only authorized users can enter, approve, or review risk data.
- **Audit Trails** — All data submissions, modifications, and approvals should be logged with timestamps and user details to ensure accountability.
- **Data Encryption** — Sensitive risk data should be securely stored and transmitted using encryption standards to prevent unauthorized access.

### 5. Data Storage & Backup

- **Initial Storage Capacity** — The system should be designed to handle current data volume efficiently, with an estimated **storage requirement of at least 5 years of historical risk reports**.
- **Backup Policy** — Automated daily backups should be implemented to prevent data loss, with a **minimum retention period of 1 year** for rollback purposes.

## Success Criteria

The solution will be considered successful if it achieves the following:

1. **Reduction in Reporting Time** — Risk data collection and report generation time should be reduced by at least 50% compared to the current manual process.
2. **Improved Data Accuracy** — The system should minimize data entry errors, ensuring at least 95% accuracy in submitted risk data.
3. **Timely Data Submission** — At least 90% of branches and departments should submit risk data on or before the deadline without follow-ups.
4. **Enhanced User Adoption** — At least 95% of designated users (Makers, Checkers, and Risk Department staff) should actively use the system within the first six months.
5. **System Uptime & Performance** — The system should maintain at least 99.9% uptime, with risk data processing completed within 3 seconds per submission.
6. **Scalability & Adaptability** — The system should support future growth, allowing seamless addition of new branches, departments, and users without performance issues.

Achieving these criteria will demonstrate the solution's effectiveness in streamlining risk reporting, enhancing decision-making, and strengthening the overall risk management framework.

## Implementation Date

In alignment with the requirements outlined in the Operational Risk Management Directions (No. 04 of 2024), the implementation of the Risk Management Information System (MIS) should be completed by August 2025. This timeline ensures compliance with regulatory expectations while allowing sufficient time for system development, testing, user training, and deployment.

## Assumptions and Constraints

### Assumptions

1. **User Readiness** — Branches, departments, and the Risk Department will actively participate in training and system adoption to ensure a smooth transition.
2. **Data Standardization** — All required risk data formats will be standardized to facilitate smooth data migration from Excel-based reporting to the new system.
3. **Infrastructure Support** — The IT department will ensure the necessary hardware, network, and security infrastructure is in place to support the system.
4. **Integration Feasibility** — The new system will be compatible with existing IT systems, minimizing integration challenges.

### Constraints

1. **Budget Limitations** — The implementation must stay within the allocated budget, which may impact system features, third-party solutions, or customizations.
2. **Timeframe for Development** — The system must be completed and operational by **August 2025**, limiting the time available for extensive customization or iterations.
3. **Resource Availability** — The project depends on internal IT and risk management teams, whose workload and availability may impact the project timeline.
4. **Change Management & Adoption** — Resistance to change from end users may require additional training and stakeholder engagement to ensure smooth adoption.

## Project Description

The project includes automating the current Excel-based risk reporting processes for various critical reports. Specifically, the automation will cover the following reports:

- Key Risk Indicators (KRI) — Monthly
- Self-Assessment Questionnaire (SAQ) — Monthly
- Loss Event Report — Monthly
- Risk Register — Monthly

### System Modules

1. **Module A: For risk management staff**
   - Access: View all reports across branches and departments.
   - Reporting: Download reports in Excel format.
   - Risk Data Management: Add, edit, and remove KRI, SAQ, and Risk Trigger/Event (in Risk Register).

2. **Module B: For Branch/Department staff**
   - Access: View reports only for their respective branch/department.
   - Reporting: Download past reports for reference.
   - Data Entry: Update and submit all risk reports.

### User Profiles

- **Branch/Department Maker** — Enters and submits risk reports.
- **Branch/Department Checker** — Reviews and approves risk reports before submission.
- **Risk Department Maker** — Creates and updates KRIs, SAQs, etc., and reviews Branch/Department risk assessments.
- **Risk Department Checker** — Reviews and approves KRIs, SAQs, etc., and reviews Branch/Department risk assessments.
- **Auditor** — Views all reports but cannot edit or submit data.

### System Architecture

1. **User Interface (UI) Layer**
   - Web-based application accessible via browser.
   - Role-based dashboards for different user profiles.
   - Secure login with authentication (Username & Password).

2. **Database Layer**
   - Stores risk data, reports, user profiles, and audit logs.
   - Supports data retrieval, filtering, and historical report access.

3. **Integration Layer**
   - Email notifications for report deadlines and approvals.

4. **Security & Compliance**
   - Role-based access control (RBAC).
   - Audit logs for tracking data changes and approvals.

### Process Flow: Key Risk Indicators (KRI) / Self-Assessment Questionnaire (SAQ) / Risk Register — Monthly

1. **Module A: For risk management staff**
   - Access: View all reports across branches and departments.
   - Reporting: Download reports in Excel format.
   - Risk Data Management: Add, edit, and remove KRI, SAQ, and Risk Trigger/Event (in Risk Register).

2. **Module B: For Branch/Department staff**
   - Access: View reports only for their respective branch/department.
   - Reporting: Download past reports for reference.
   - Data Entry: Update and submit all risk reports.

*(Note: The original document included flow chart diagrams illustrating these two process flows. The diagrams have been omitted here; the textual description of each module's access and responsibilities above captures the same content.)*

### Report Requests

1. **Audit Logs** — Audit logs should be available for all activities and accessible based on Risk Management staff user IDs (UIDs).
2. **Branch Department Report (BM Authorized Date)** — A report showing the dates when branch departments submitted and the Branch Manager authorized the data.
3. **Branch-wise KRI and SAQ Report** — A report displaying Key Risk Indicators (KRI) and Self-Assessment Questionnaires (SAQ) by branch.

**Note:** All reports should be downloadable in Excel format.

### Attached Excel Formats (Referenced in Original Document)

- Branch KRI - Jan 2025
- Branch SAQ - Jan 2025
- Department KRI - Jan 2025
- Department SAQ - Jan 2025
- Admin - SAQ Department Jan 2025
- KRI HR Jan 2025
- KRI Operations Jan 2025

### Screens Referenced in Original Document

The original document included screenshots for the following screens (images omitted; screen names and purposes retained):

- **Branch or department maker input screen** — Used by Makers to input risk data.
- **Branch or department checker authorizing screen** — Used by Checkers to review and authorize submitted data.
- **Risk Department screen** — Used by the Risk Department to manage and review submissions.
- **Summary report screen (Excel download)** — Used to generate and download summary reports in Excel format.

### Loss Event Report — Monthly

The process remains the same as for KRIs, with the only change being the attached format:

1. Root Cause Analysis (RCA) format
2. Risk Incidents analysis (RIA) format

## Contact for Clarifications

If you need any clarifications, please contact Sandaruwan from the Risk Department (Ext. No. 2363).
