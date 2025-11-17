// Copyright 2025 Google LLC
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     https://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// This file defines the 'delete-deployment' (alias 'undeploy') command for
// the clasp CLI.
import { Command } from 'commander';
import inquirer from 'inquirer';
import { intl } from '../intl.js';
import { isInteractive, withSpinner } from './utils.js';
export const command = new Command('delete-deployment')
    .alias('undeploy')
    .description('Delete a deployment of a project')
    .arguments('[deploymentId]')
    .option('-a, --all', 'Undeploy all deployments')
    .action(async function (deploymentId) {
    var _a;
    const options = this.optsWithGlobals();
    const clasp = options.clasp;
    const removeAll = options.all;
    const deletedDeploymentIds = [];
    const deleteDeployment = async (id) => {
        const spinnerMsg = intl.formatMessage({ id: "d6mKEK", defaultMessage: [{ type: 0, value: "Deleting deployment..." }] });
        await withSpinner(spinnerMsg, async () => {
            return clasp.project.undeploy(id);
        });
        deletedDeploymentIds.push(id);
        if (!options.json) {
            const successMessage = intl.formatMessage({ id: "GbtFER", defaultMessage: [{ type: 0, value: "Deleted deployment " }, { type: 1, value: "id" }] }, { id });
            console.log(successMessage);
        }
    };
    if (removeAll) {
        const spinnerMsg = intl.formatMessage({ id: "baVdkq", defaultMessage: [{ type: 0, value: "Fetching deployments..." }] });
        const deployments = await withSpinner(spinnerMsg, async () => {
            return await clasp.project.listDeployments();
        });
        deployments.results = deployments.results.filter(deployment => { var _a; return ((_a = deployment.deploymentConfig) === null || _a === void 0 ? void 0 : _a.versionNumber) !== undefined; });
        for (const deployment of deployments.results) {
            const id = deployment.deploymentId;
            if (!id) {
                continue;
            }
            await deleteDeployment(id);
        }
        if (options.json) {
            console.log(JSON.stringify({ deletedDeploymentIds }, null, 2));
            return;
        }
        const successMessage = intl.formatMessage({ id: "cE8hF0", defaultMessage: [{ type: 0, value: "Deleted all deployments." }] });
        console.log(successMessage);
        return;
    }
    if (!deploymentId) {
        const deployments = await clasp.project.listDeployments();
        deployments.results = deployments.results.filter(deployment => { var _a; return ((_a = deployment.deploymentConfig) === null || _a === void 0 ? void 0 : _a.versionNumber) !== undefined; });
        if (deployments.results.length === 1) {
            deploymentId = (_a = deployments.results[0].deploymentId) !== null && _a !== void 0 ? _a : undefined;
        }
        else if (isInteractive()) {
            const prompt = intl.formatMessage({ id: "LDbUYg", defaultMessage: [{ type: 0, value: "Delete which deployment?" }] });
            const choices = deployments.results.map(deployment => {
                var _a, _b;
                return ({
                    name: `${deployment.deploymentId} - ${(_b = (_a = deployment.deploymentConfig) === null || _a === void 0 ? void 0 : _a.description) !== null && _b !== void 0 ? _b : ''}`,
                    value: deployment.deploymentId,
                });
            });
            const answer = await inquirer.prompt([
                {
                    choices: choices,
                    message: prompt,
                    name: 'deploymentId',
                    pageSize: 30,
                    type: 'list',
                },
            ]);
            deploymentId = answer.deploymentId;
        }
    }
    if (!deploymentId) {
        if (options.json) {
            console.log(JSON.stringify({ deletedDeploymentIds: [] }, null, 2));
            return;
        }
        const msg = intl.formatMessage({ id: "UufukD", defaultMessage: [{ type: 0, value: "No deployments found." }] });
        this.error(msg);
    }
    await deleteDeployment(deploymentId);
    if (options.json) {
        console.log(JSON.stringify({ deletedDeploymentIds }, null, 2));
    }
});
