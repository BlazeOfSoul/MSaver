import { CategoryType } from '../../enums/transaction-type.enum';

export interface Category {
    id: string;
    name: string;
    type: CategoryType;
}
