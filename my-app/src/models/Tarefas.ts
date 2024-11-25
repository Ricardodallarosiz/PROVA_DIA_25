export interface Tarefa {
    id: number; 
    titulo: string; 
    descricao: string; 
    status: string; 
    categoria: {
        id: number; 
        nome: string; 
    };
}
