import { Injectable } from "@angular/core";

import { MemberEditComponent } from '../members/member-edit/member-edit.component';
import { CanDeactivate } from '@angular/router';

@Injectable()
export class PreventUnsavedChanges implements CanDeactivate<MemberEditComponent>
{
    // create a pop up box if there are unsaved changes on the edit page
    canDeactivate(component: MemberEditComponent) {
        if (component.editForm.dirty) {
            return confirm('Are you sure you want to contine? Any unsaved changes will be lost');
        }

        return true;
    }
}