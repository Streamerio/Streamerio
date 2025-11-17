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
// This file defines the 'list-deployments' (alias 'deployments') command for
// the clasp CLI.
import { Command } from 'commander';
import { intl } from '../intl.js';
import { withSpinner } from './utils.js';
export const command = new Command('list-deployments')
    .alias('deployments')
    .description('List deployment ids of a script')
    .argument('[scriptId]', 'Apps Script ID to list deployments for')
    .action(async function (scriptId) {
    const options = this.optsWithGlobals();
    const clasp = options.clasp;
    if (scriptId) {
        clasp.withScriptId(scriptId);
    }
    const spinnerMsg = intl.formatMessage({ id: "baVdkq", defaultMessage: [{ type: 0, value: "Fetching deployments..." }] });
    const deployments = await withSpinner(spinnerMsg, () => clasp.project.listDeployments());
    if (options.json) {
        const deploymentOutput = deployments.results.map(deployment => {
            var _a, _b;
            return ({
                deploymentId: deployment.deploymentId,
                versionNumber: (_a = deployment.deploymentConfig) === null || _a === void 0 ? void 0 : _a.versionNumber,
                description: (_b = deployment.deploymentConfig) === null || _b === void 0 ? void 0 : _b.description,
            });
        });
        console.log(JSON.stringify(deploymentOutput, null, 2));
        return;
    }
    if (!deployments.results.length) {
        const msg = intl.formatMessage({ id: "q2/XsW", defaultMessage: [{ type: 0, value: "No deployments." }] });
        console.log(msg);
        return;
    }
    const successMessage = intl.formatMessage({ id: "EJQ1WM", defaultMessage: [{ type: 0, value: "Found " }, { type: 6, value: "count", options: { one: { value: [{ type: 7 }, { type: 0, value: " deployment" }] }, other: { value: [{ type: 7 }, { type: 0, value: " deployments" }] } }, offset: 0, pluralType: "cardinal" }, { type: 0, value: "." }] }, {
        count: deployments.results.length,
    });
    console.log(successMessage);
    deployments.results
        .filter(d => d.deploymentConfig && d.deploymentId)
        .forEach(d => {
        var _a, _b;
        const versionString = ((_a = d.deploymentConfig) === null || _a === void 0 ? void 0 : _a.versionNumber) ? `@${d.deploymentConfig.versionNumber}` : '@HEAD';
        const description = ((_b = d.deploymentConfig) === null || _b === void 0 ? void 0 : _b.description) ? `- ${d.deploymentConfig.description}` : '';
        console.log(`- ${d.deploymentId} ${versionString} ${description}`);
    });
});
