// Testing CI

import jobs.generation.Utilities

def project = GithubProject
def branch = GithubBranchName
def isPr = true
def containerOs = 'Debian'
def newJobName = Utilities.getFullJobName(project, containerOs, isPr)

def newJob = job(newJobName) {
    steps {
        shell(".build-and-test.sh")
    }
}

Utilities.setMachineAffinity(newJob, 'Ubuntu16.04', 'latest-or-auto-docker')
Utilities.standardJobSetup(newJob, project, isPr, "*/${branch}")
Utilities.addGithubPRTriggerForBranch(newJob, branch, containerOs)
